using Newtonsoft.Json;
using System;
using System.Xml;

namespace SparkCode.Data
{
    public static class XmlToJson
    {
        public static string Convert(string xml)
        {
            if (string.IsNullOrWhiteSpace(xml))
            {
                throw new ArgumentNullException(nameof(xml));
            }

            var doc = new XmlDocument();
            doc.LoadXml(xml);

            var json = JsonConvert.SerializeXmlNode(doc);

            return json;
        }
    }
}