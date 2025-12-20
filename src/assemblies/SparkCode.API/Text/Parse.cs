using Microsoft.Xrm.Sdk;
using System;

namespace SparkCode.API.Text
{
    /// <displayName>Parse</displayName>
    /// <summary>
    /// Converts a text value to diferent data types
    /// </summary>
    /// <param name="Text" type="string">Text to be parsed</param>
    /// <param name="Integer" type="integer" direction="output">Text converted to integer</param>
    /// <example>
    /// To convert a number in text format to an integer, add the Text input parameter with the text value "12345". 
    /// The Results output parameter will return the integer value 12345.
    /// ![How to use this custom API](docs/img/TextToInteger01.png "How to use this custom API")
    /// </example>
    public class Parse : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var ctx = new Context(serviceProvider);

            // API Inputs
            string text = ctx.GetInputParameter<string>("Text", true);

            // API Outputs
            ctx.SetOutputParameter("Integer", int.Parse(text));
        }

    }
}
