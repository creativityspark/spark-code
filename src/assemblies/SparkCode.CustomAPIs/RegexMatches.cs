using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SparkCode.CustomAPIs
{
    public class RegexMatches : IPlugin
    {
        Context ctx = new Context();
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            ctx = new Context(serviceProvider);

            var input = context.InputParameters["Input"] as string;
            var pattern = context.InputParameters["Pattern"] as string;
            var opt = context.InputParameters["Options"];
            int options = opt != null ? (int)opt : 0;

            List<CaptureInfo> capturesList = new List<CaptureInfo>();
            string captures = string.Empty;

            var regex = new Regex(pattern, (RegexOptions)options);
            var matches = regex.Matches(input);

            foreach (Match match in matches)
            {
                capturesList.Add(new CaptureInfo { Value = match.Value, Index = match.Index, Length= match.Length });
            }

            captures= JsonConvert.SerializeObject(capturesList, Formatting.Indented);

            context.OutputParameters["Captures"] = captures;
        }
    }
}
