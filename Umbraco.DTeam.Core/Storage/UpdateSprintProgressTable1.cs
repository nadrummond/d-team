using System;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.DTeam.Core.Storage
{
    [Migration("1.0.3", 1, Migrations.MigrationProductName)]
    public class UpdateSprintProgressTable1 : MigrationBase
    {
        public UpdateSprintProgressTable1(ISqlSyntaxProvider syntax, ILogger logger)
            : base(syntax, logger)
        { }

        public override void Up()
        {
            Execute.Code(database =>
            {
                var existing = SprintProgress.Get(55);
                var compare = new DateTime(2017, 03, 22, 00, 00, 00);
                if (existing.Any(x => x.DateTime == compare)) return string.Empty;

                database.Insert(new SprintProgressDto { SprintId = 55, DateTime = new DateTime(2017, 03, 22, 00, 00, 00), JsonData = "{\"Points\":{\"Open\":59.5,\"In Progress\":25.5,\"Review\":36.0,\"Reopened\":1.0,\"Fixed\":41.0,\"Other\":3.0},\"Unscheduled\":23.0}" });

                return string.Empty;
            });
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}
