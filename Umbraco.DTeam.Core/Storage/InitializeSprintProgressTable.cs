using System;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.DTeam.Core.Storage
{
    [Migration("1.0.1", 1, Migrations.MigrationProductName)]
    public class InitializeSprintProgressTable : MigrationBase
    {
        public InitializeSprintProgressTable(ISqlSyntaxProvider syntax, ILogger logger)
            : base(syntax, logger)
        { }

        public override void Up()
        {
            Execute.Code(database =>
            {
                var existing = SprintProgress.Get(55);
                if (existing.Any()) return string.Empty;

                database.Insert(new SprintProgressDto { SprintId = 55, DateTime = new DateTime(2017, 03, 20, 12, 00, 00), JsonData = "{\"Points\":{\"Open\":66.5,\"In Progress\":29.5,\"Review\":32.0,\"Reopened\":3.0,\"Fixed\":29.0,\"Other\":3.0},\"Unscheduled\":23.0}" });
                database.Insert(new SprintProgressDto { SprintId = 55, DateTime = new DateTime(2017, 03, 21, 00, 00, 00), JsonData = "{\"Points\":{\"Open\":66.5,\"In Progress\":29.5,\"Review\":32.0,\"Reopened\":3.0,\"Fixed\":29.0,\"Other\":3.0},\"Unscheduled\":23.0}" });
                database.Insert(new SprintProgressDto { SprintId = 55, DateTime = new DateTime(2017, 03, 21, 12, 00, 00), JsonData = "{\"Points\":{\"Open\":66.5,\"In Progress\":29.5,\"Review\":32.0,\"Reopened\":3.0,\"Fixed\":29.0,\"Other\":3.0},\"Unscheduled\":23.0}" });

                return string.Empty;
            });
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}
