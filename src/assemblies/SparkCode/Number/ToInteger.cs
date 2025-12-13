using Microsoft.Xrm.Sdk;
using System;
using XrmEntitySerializer;

namespace SparkCode.API.Text
{
    /// <displayName>To Integer</displayName>
    /// <summary>
    /// Converts a text value to an integer
    /// </summary>
    /// <param name="Text" type="string">Text to be converted to integer</param>
    /// <param name="Results" type="integer" direction="output">Text converted to integer</param>
    /// <example>
    /// To convert a number in text format to an integer, add the Text input parameter with the text value "12345". 
    /// The Results output parameter will return the integer value 12345.
    /// ![How to use this custom API](assets/image1.png "How to use this custom API")
    /// </example>
    public class ToInteger : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var ctx = new Context(serviceProvider);

            // API Inputs
            string text = ctx.GetInputParameter<string>("Text", true);

            // API Outputs
            ctx.SetOutputParameter("Results", int.Parse(text));
        }

    }
}
