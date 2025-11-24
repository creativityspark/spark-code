using Microsoft.Xrm.Sdk;
using System;

namespace SparkCode.CustomAPIs.Other
{
    public class ParseURL : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var ctx = new SparkCode.Context(serviceProvider);

            // API Inputs
            string url = ctx.GetInputParameter<string>("Url",true);

            // Run Logic
            var results = SparkCode.Other.ParseURL.Parse(ctx, url);

            // API Outputs
            ctx.SetOutputParameter("Results",results);
        }
    }
}
