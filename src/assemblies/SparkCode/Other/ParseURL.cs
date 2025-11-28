using Microsoft.Xrm.Sdk;
using System;
using System.Runtime.CompilerServices;

namespace SparkCode.Other
{
    public static class ParseURL
    {
        public static Entity Parse(Context ctx, string url)
        {
            var results = new Entity();
            Uri uri = new Uri(url);

            results = new Entity();
            var query = new Entity();
            results["scheme"] = uri.Scheme;
            results["host"] = uri.Host;
            results["port"] = uri.Port;
            results["absolutePath"] = uri.AbsolutePath;
            results["fragment"] = uri.Fragment.Length > 0 ? uri.Fragment.Substring(1) : string.Empty;

            // parse query parameters
            var queryParameters = System.Web.HttpUtility.ParseQueryString(uri.Query);
            foreach (string key in queryParameters)
            {
                query[key] = queryParameters[key];
            }

            results["query"] = query;

            return results;
        }
    }
}
