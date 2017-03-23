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

        private static UmbracoDatabase Database => ApplicationContext.Current.DatabaseContext.Database;

        public static void Save(SprintProgress progress)
        {
            var sql = new Sql("SELECT sprintId, dateTime, jsonData FROM dSprintProgress WHERE sprintId=@sprintId AND dateTime=@dateTime", new { sprintId = progress.SprintId, dateTime = progress.DateTime });
            var dto = Database.Fetch<SprintProgressDto>(sql).FirstOrDefault();
            if (dto == null)
            {
                dto = new SprintProgressDto
                {
                    SprintId = progress.SprintId,
                    DateTime = progress.DateTime,
                    JsonData = JsonConvert.SerializeObject(new JsonData { Points = progress.Points, Unscheduled = progress.Unscheduled })
                };
                Database.Insert(dto);
            }
            else
            {
                dto.JsonData = JsonConvert.SerializeObject(new JsonData { Points = progress.Points, Unscheduled = progress.Unscheduled });
                Database.Update(dto);
            }
        }

        public static IEnumerable<SprintProgress> Get(int sprintId)
        {
            var sql = new Sql("SELECT sprintId, dateTime, jsonData FROM dSprintProgress WHERE sprintId=@sprintId ORDER BY dateTime", new { sprintId });
            var dtos = Database.Fetch<SprintProgressDto>(sql);
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
