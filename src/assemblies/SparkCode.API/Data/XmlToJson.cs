using Microsoft.Xrm.Sdk;
using System;

namespace SparkCode.API.Data
{
    /// <displayName>XML To JSON</displayName>
    /// <summary>Converts an XML string to JSON.</summary>
    /// <param name="Xml" type="string">XML payload to convert.</param>
    /// <param name="Results" type="string" direction="output">JSON representation of the XML payload.</param>
    public class XmlToJson : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var ctx = new Context(serviceProvider);

            // API Inputs
            string xml = ctx.GetInputParameter<string>("Xml", true);

            // Run Logic
            string results = SparkCode.Data.XmlToJson.Convert(ctx, xml);

            // API Outputs
            ctx.SetOutputParameter("Results", results);
        }
    }
}