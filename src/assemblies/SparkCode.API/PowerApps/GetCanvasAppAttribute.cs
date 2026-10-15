using Microsoft.Xrm.Sdk;
using System;

namespace SparkCode.API.PowerApps
{
    /// <summary>
    /// A plugin that returns the string value of an attribute of a canvas app.
    /// </summary>
    /// <displayName>Get Canvas App Attribute</displayName>
    /// <param name="Attribute" type="string">Name of the attribute that needs to be retrieved.</param>
    /// <param name="App" type="string">Unique name of the canvas app whose attribute needs to be retrieved.</param>
    /// <param name="Output" type="string" direction="output">String value of the attribute.</param>
    public class GetCanvasAppAttribute : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var ctx = new Context(serviceProvider);

            // API Inputs
            string attribute = ctx.GetInputParameter<string>("Attribute", true);
            string app = ctx.GetInputParameter<string>("App", true);

            // Run Logic
            var value = ctx.Service.GetCanvasAppAttribute<object>(app, attribute);
            var output = Conversions.ToStringValue(value);

            // API Outputs
            if (output != null)
            {
                ctx.SetOutputParameter("Output", output);
            }
        }
    }
}
