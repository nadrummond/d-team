using System;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.DTeam.Core
{
    public static class Extensions
    {        
        public static double MaxOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector, double defaultValue)
        {
            if (source.Any()) return source.Max(selector);
            return defaultValue;
        }
    }
}