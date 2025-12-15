using Microsoft.Xrm.Sdk;
using System;

namespace SparkCode.API.Text
{
    /// <displayName>Parse URL</displayName>
    /// <summary>Parses a URL and returns its parts</summary>
    /// <param name="Url" type="string">Url to be parsed</param>
    /// <param name="Results" type="expando" direction="output">All the parts contained in the Url</param>
    public class ParseURL : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var ctx = new Context(serviceProvider);

            // API Inputs
            string url = ctx.GetInputParameter<string>("Url", true);

            // Run Logic
            var results = SparkCode.Text.ParseURL.Parse(ctx, url);

            // API Outputs
            ctx.SetOutputParameter("Results", results);
            ctx.SetOutputParameter("ResultsJson", results.ToJson());
        }
    }
}
