using System;

namespace Umbraco.DTeam.Core.Auth
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Gets Unix Timestamp from DateTime object
        /// </summary>
        /// <param name="dateTime">The DateTime object</param>
        public static double ToUnixTimestamp(this DateTime dateTime)
        {
            return (TimeZoneInfo.ConvertTimeToUtc(dateTime) -
                    new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }

        /// <summary>
        /// Gets DateTime object from Unix Timestamp
        /// </summary>
        /// <param name="unixTime">The Unix Timestamp</param>
        public static DateTime FromUnixTime(this double unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(unixTime);
        }
    }
}
