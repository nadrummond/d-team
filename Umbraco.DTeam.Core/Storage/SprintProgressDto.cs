using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.DTeam.Core.Storage
{
    [TableName("dSprintProgress")]
    //[PrimaryKey("id", autoIncrement = true)]
    [ExplicitColumns]
    public class SprintProgressDto
    {
        //[Column("id")]
        //[NullSetting(NullSetting = NullSettings.NotNull)]
        //[PrimaryKeyColumn(AutoIncrement = true, Clustered = true, Name="PK_dSprintProgress")]
        //public int Id { get; set; }

        [Column("sprintId")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [Index(IndexTypes.UniqueNonClustered, Name = "IX_dSprintProgress", ForColumns = "sprintId, dateTime")]
        public int SprintId { get; set; }

        [Column("dateTime")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public DateTime DateTime { get; set; }

        [Column("jsonData")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [Length(512)]
        public string JsonData { get; set; }
    }
}
