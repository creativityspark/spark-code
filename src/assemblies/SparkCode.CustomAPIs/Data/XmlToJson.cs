using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using System;
using System.Xml;

namespace SparkCode.CustomAPIs.Data
{
    /// <summary>
    /// Converts XML string to JSON string
    /// Based on: https://www.newtonsoft.com/json/help/html/ConvertXmlToJson.htm
    /// </summary>
    public class XmlToJson : IPlugin
    {
        Context ctx = new Context();

        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            ctx = new Context(serviceProvider);

            // Custom API Inputs
            string xml = context.InputParameters["Xml"] as string ?? throw new ArgumentNullException($"Xml is required");

            string json = Convert(ctx, xml);

            // Set OutputParameters values
            context.OutputParameters["Json"] = json;
        }

        public string Convert(Context ctx, string xml)
        {
            // Trace input parameters
            ctx.Trace($"Xml: {xml}");

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            string json = JsonConvert.SerializeXmlNode(doc);

            ctx.Trace($"JSon: {json}");
            return json;
        }
    }
}
