using Microsoft.Xrm.Sdk;
using System;
using System.Text.RegularExpressions;

namespace SparkCode.CustomAPIs.Text
{
    public class Base64Encode : IPlugin
    {
        Context ctx = new Context();

        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            ctx = new Context(serviceProvider);

            string input = context.InputParameters["Input"] as string;
            ctx.Trace($"Input: {input}");

            // Convert the input string to a byte array
            byte[] data = System.Text.Encoding.UTF8.GetBytes(input);
            // Convert the byte array to a Base64 encoded string
            string output = Convert.ToBase64String(data);

            ctx.Trace($"Output: {output}");
            context.OutputParameters["Output"] = output;
        }
    }
}
