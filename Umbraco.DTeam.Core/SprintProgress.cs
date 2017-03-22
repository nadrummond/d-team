using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Umbraco.DTeam.Core.Storage;

namespace Umbraco.DTeam.Core
{
    public class SprintProgress
    {
        public int SprintId { get; set; }
        public DateTime DateTime { get; set; }
        public Dictionary<string, double> Points { get; set; }
        public double Unscheduled { get; set; }
        public double TotalPoints { get; set; }

        public class JsonData
        {
            public Dictionary<string, double> Points { get; set; }
            public double Unscheduled { get; set; }
        }

        public static void Save(SprintProgress progress)
        {
            var dto = new SprintProgressDto
            {
                SprintId = progress.SprintId,
                DateTime = progress.DateTime,
                JsonData = JsonConvert.SerializeObject(new JsonData { Points = progress.Points, Unscheduled = progress.Unscheduled })
            };
            ApplicationContext.Current.DatabaseContext.Database.Insert(dto);
        }

        public static IEnumerable<SprintProgress> Get(int sprintId)
        {
            var sql = new Sql("SELECT sprintId, dateTime, jsonData FROM dSprintProgress WHERE sprintId=@sprintId ORDER BY dateTime", new { sprintId });
            var dtos = ApplicationContext.Current.DatabaseContext.Database.Fetch<SprintProgressDto>(sql);
            return dtos.Select(x =>
            {
                var jsonData = JsonConvert.DeserializeObject<JsonData>(x.JsonData);
                var progress = new SprintProgress
                {
                    SprintId = x.SprintId,
                    DateTime = x.DateTime,
                    Points = jsonData.Points,
                    Unscheduled = jsonData.Unscheduled
                };

                progress.TotalPoints = progress.Points.Sum(p => p.Value);

                return progress;
            });
        }
    }
}
