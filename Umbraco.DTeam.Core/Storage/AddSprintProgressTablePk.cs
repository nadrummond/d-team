using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.Migrations.Syntax.Alter;
using Umbraco.Core.Persistence.Migrations.Syntax.Create;
using Umbraco.Core.Persistence.Migrations.Syntax.Delete;
using Umbraco.Core.Persistence.Migrations.Syntax.Execute;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.DTeam.Core.Storage
{
    [Migration("1.0.4", 1, Migrations.MigrationProductName)]
    public class AddSprintProgressTablePk : MigrationBase
    {
        public AddSprintProgressTablePk(ISqlSyntaxProvider sqlSyntax, ILogger logger) 
            : base(sqlSyntax, logger)
        { }

        public override void Up()
        {
            // defer, because we are making decisions based upon what's in the database
            Execute.Code(MigrationCode);
        }

        // if no Id then insert ID and declare PK
        // also delete duplicates

        private string MigrationCode(Database database)
        {
            var columns = SqlSyntax.GetColumnsInSchema(database).ToArray();

            if (columns.Any(x => x.TableName.InvariantEquals("dSprintProgress") && x.ColumnName.InvariantEquals("id")))
                return string.Empty;

            using (var transaction = database.GetTransaction())
            {
                // get them all
                var sql = new Sql("SELECT sprintId, dateTime, jsonData FROM dSprintProgress ORDER BY sprintId, dateTime");
                var dtos = database.Fetch<SprintProgressDto>(sql);

                // drop and recreate table with PK
                database.Execute("DROP TABLE dSprintProgress");
                var localContext = new LocalMigrationContext(Context.CurrentDatabaseProvider, database, SqlSyntax, Logger);

                localContext.Create
                    .Table("dSprintProgress")
                    .WithColumn("id").AsInt32().NotNullable().Identity()
                    .WithColumn("sprintId").AsInt32().NotNullable()
                    .WithColumn("dateTime").AsDateTime().NotNullable()
                    .WithColumn("jsonData").AsString(512).NotNullable();
                localContext.Create
                    .PrimaryKey("PK_dSprintProgress").OnTable("dSprintProgress").Column("id");
                localContext.Create
                    .Index("IX_dSprintProgress")
                    .OnTable("dSprintProgress")
                    .OnColumn("sprintId").Ascending()
                    .OnColumn("dateTime").Ascending()
                    .WithOptions().Unique()
                    .WithOptions().NonClustered();

                localContext.ExecuteSql();
                localContext.Clear();

                // insert back with pk and no duplicate
                SprintProgressDto prev = null;
                foreach (var dto in dtos)
                {
                    if (prev == null)
                    {
                        prev = dto;
                        continue;
                    }
                    if (dto.DateTime != prev.DateTime)
                    {
                        database.Insert(prev);
                    }
                    prev = dto;
                }
                if (prev != null)
                    database.Insert(prev);

                transaction.Complete();
            }

            return string.Empty;
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }

    // fixme - move!
    internal class MigrationContext2 : IMigrationContext
    {
        public MigrationContext2(DatabaseProviders databaseProvider, Database database, ILogger logger)
        {
            Expressions = new Collection<IMigrationExpression>();
            CurrentDatabaseProvider = databaseProvider;
            Database = database;
            Logger = logger;
        }

        public ICollection<IMigrationExpression> Expressions { get; set; }

        public DatabaseProviders CurrentDatabaseProvider { get; }

        public Database Database { get; }

        public ILogger Logger { get; }
    }

    // fixme - merge!
    internal class LocalMigrationContext : MigrationContext2
    {
        private readonly ISqlSyntaxProvider _sqlSyntax;

        public LocalMigrationContext(DatabaseProviders databaseProvider, Database database, ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(databaseProvider, database, logger)
        {
            _sqlSyntax = sqlSyntax;
        }

        public IExecuteBuilder Execute => new ExecuteBuilder(this, _sqlSyntax);

        public IDeleteBuilder Delete => new DeleteBuilder(this, _sqlSyntax);

        public IAlterSyntaxBuilder Alter => new AlterSyntaxBuilder(this, _sqlSyntax);

        public ICreateBuilder Create => new CreateBuilder(this, _sqlSyntax);

        public string GetSql()
        {
            var sb = new StringBuilder();
            foreach (var sql in Expressions.Select(x => x.Process(Database)))
            {
                sb.Append(sql);
                sb.AppendLine();
                sb.AppendLine("GO");
            }
            return sb.ToString();
        }

        public void ExecuteSql()
        {
            foreach (var sql in Expressions.Select(x => x.Process(Database)))
            {
                if (string.IsNullOrWhiteSpace(sql))
                    continue;

                var sb = new StringBuilder();
                using (var reader = new StringReader(sql))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        line = line.Trim();
                        if (line.Equals("GO", StringComparison.OrdinalIgnoreCase))
                        {
                            //Execute the SQL up to the point of a GO statement
                            var exeSql = sb.ToString();
                            LogHelper.Info<LocalMigrationContext>("Exec: " + exeSql);
                            Database.Execute(exeSql);

                            //restart the string builder
                            sb.Remove(0, sb.Length);
                        }
                        else
                        {
                            sb.AppendLine(line);
                        }
                    }
                    //execute anything remaining
                    if (sb.Length > 0)
                    {
                        var exeSql = sb.ToString();
                        LogHelper.Info<LocalMigrationContext>("Exec: " + exeSql);
                        Database.Execute(exeSql);
                    }
                }
            }
        }

        public void Clear()
        {
            Expressions.Clear();
        }
    }
}
