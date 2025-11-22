using Microsoft.Xrm.Sdk;
using System;
using System.Text.RegularExpressions;

namespace SparkCode.CustomAPIs.Text
{
    public class RegexMatch : IPlugin
    {
        Context ctx = new Context();

        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            ctx = new Context(serviceProvider);

            string input = context.InputParameters["Input"] as string;
            string pattern = context.InputParameters["Pattern"] as string;

            Match match = Regex.Match(input, pattern);
            var success = match.Success;
            var index = match.Index;
            var value = match.Value;

            context.OutputParameters["Success"] = success;
            context.OutputParameters["Index"] = index;
            context.OutputParameters["Value"] = value;
        }
    }
}
