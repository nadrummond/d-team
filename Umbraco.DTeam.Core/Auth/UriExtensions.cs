﻿using System;

namespace Umbraco.DTeam.Core.Auth
{
    public static class UriExtensions
    {
        public static string CleanPathAndQuery(this Uri uri)
        {
            //sometimes the request path may have double slashes so make sure to normalize this
            return uri.PathAndQuery.Replace("//", "/");
        }
    }
}
