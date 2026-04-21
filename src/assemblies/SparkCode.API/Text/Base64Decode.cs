using Microsoft.Xrm.Sdk;
using System;
using System.Text;

namespace SparkCode.API.Text
{
    /// <displayName>Base64 Decode</displayName>
    /// <summary>
    /// Converts a Base64 encoded string into a UTF8 string.
    /// </summary>
    /// <param name="Input" type="string">Base64 text to be converted</param>
    /// <param name="Output" type="string" direction="output">Decoded UTF8 string</param>
    /// <example>
    /// To decode "Hello World" from Base64, pass the Input parameter as "SGVsbG8gV29ybGQ=".
    /// The Output parameter will return "Hello World".
    /// </example>
    public class Base64Decode : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var ctx = new Context(serviceProvider);

            // API Inputs
            string base64 = ctx.GetInputParameter<string>("Input", true);

            // Run Logic
            string result = Encoding.UTF8.GetString(Convert.FromBase64String(base64));

            // API Outputs
            ctx.SetOutputParameter("Output", result);
        }
    }
}