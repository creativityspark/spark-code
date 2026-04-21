using Microsoft.Xrm.Sdk;
using System;
using System.Text;

namespace SparkCode.API.Text
{
    /// <displayName>Base64 Encode</displayName>
    /// <summary>
    /// Converts a UTF8 string into a Base64 encoded string.
    /// </summary>
    /// <param name="Input" type="string">UTF8 text to be converted</param>
    /// <param name="Output" type="string" direction="output">Base64 encoded string</param>
    /// <example>
    /// To encode "Hello World" to Base64, pass the Input parameter as "Hello World".
    /// The Output parameter will return "SGVsbG8gV29ybGQ=".
    /// </example>
    public class Base64Encode : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var ctx = new Context(serviceProvider);

            // API Inputs
            string input = ctx.GetInputParameter<string>("Input", true);

            // Run Logic
            string result = Convert.ToBase64String(Encoding.UTF8.GetBytes(input));

            // API Outputs
            ctx.SetOutputParameter("Output", result);
        }
    }
}