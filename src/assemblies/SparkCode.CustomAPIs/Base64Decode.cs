using Microsoft.Xrm.Sdk;
using System;
using System.Text.RegularExpressions;

namespace SparkCode.CustomAPIs
{
    public class Base64Decode : IPlugin
    {
        Context ctx = new Context();

        /// <summary>
        /// Converts a base 64 encoded string to a normal string
        /// </summary>
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            ctx = new Context(serviceProvider);

            string input = context.InputParameters["Input"] as string;
            ctx.Trace($"Input: {input}");

            byte[] data = Convert.FromBase64String(input);
            string output = System.Text.Encoding.UTF8.GetString(data);

            ctx.Trace($"Output: {output}");
            context.OutputParameters["Output"] = output;
        }
    }
}
