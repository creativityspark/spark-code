using Microsoft.Xrm.Sdk;
using System;
using System.Text.RegularExpressions;

namespace SparkCode.CustomAPIs
{
    public class RegexReplace : IPlugin
    {
        Context ctx = new Context();

        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            ctx = new Context(serviceProvider);

            string input = context.InputParameters["InputString"] as string;
            string pattern = context.InputParameters["RegExpPattern"] as string;
            string replacement = context.InputParameters["ReplacementText"] as string;

            string result = Regex.Replace(input, pattern, replacement);

            context.OutputParameters["Result"] = "";
        }
    }
}
