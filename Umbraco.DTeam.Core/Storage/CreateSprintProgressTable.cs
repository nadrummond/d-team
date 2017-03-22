using System;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.DTeam.Core.Storage
{
    [Migration("1.0.0", 1, Migrations.MigrationProductName)]
    public class CreateSprintProgressTable : MigrationBase
    {
        public CreateSprintProgressTable(ISqlSyntaxProvider syntax, ILogger logger)
            : base(syntax, logger)
        { }

        // fixme change when running 7.6!
        private static DatabaseContext Context => ApplicationContext.Current.DatabaseContext;

        public override void Up()
        {
            Logger.Debug<CreateSprintProgressTable>("Looking for tables.");

            var tables = SqlSyntax.GetTablesInSchema(Context.Database).ToArray();
            var dSprintProgressExists = tables.InvariantContains("dSprintProgress");

            if (!dSprintProgressExists)
            {
                // fixme this is 7.6!
                //Create.Table<SprintProgressDto>();
                Create.Table("dSprintProgress")
                    .WithColumn("sprintId").AsInt32().NotNullable()
                    .WithColumn("dateTime").AsDateTime().NotNullable()
                    .WithColumn("jsonData").AsString(512).NotNullable();
                Create.Index("IX_dSprintProgress")
                    .OnTable("dSprintProgress")
                    .OnColumn("sprintId").Ascending()
                    .OnColumn("dateTime").Ascending()
                    .WithOptions().Unique()
                    .WithOptions().NonClustered();
            }

            Logger.Debug<CreateSprintProgressTable>("Done.");
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}
