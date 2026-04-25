using Newtonsoft.Json;
using System;
using System.Xml;

namespace SparkCode.Data
{
    public static class XmlToJson
    {
        public static string Convert(Context ctx, string xml)
        {
            if (string.IsNullOrWhiteSpace(xml))
            {
                throw new ArgumentNullException(nameof(xml));
            }

            ctx?.Trace($"Xml: {xml}");

            var doc = new XmlDocument();
            doc.LoadXml(xml);

            var json = JsonConvert.SerializeXmlNode(doc);

            ctx?.Trace($"Json: {json}");
            return json;
        }
    }
}