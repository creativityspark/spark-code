using System;
using System.Xml;
using Xunit;

namespace SparkCode.Tests.Data
{
    public class XmlToJsonTests
    {
        [Fact]
        public void Convert_ValidXml_ReturnsJson()
        {
            var ctx = new Context();
            string xml = "<root><element>value</element></root>";
            string expectedJson = "{\"root\":{\"element\":\"value\"}}";

            string json = SparkCode.Data.XmlToJson.Convert(ctx, xml);
            Assert.Equal(expectedJson, json);
        }

        [Fact]
        public void Convert_InvalidXml_ThrowsException()
        {
            var ctx = new Context();
            string invalidXml = "<root><element>value</element>";

            Assert.Throws<XmlException>(() => SparkCode.Data.XmlToJson.Convert(ctx, invalidXml));
        }

        [Fact]
        public void Convert_EmptyXml_ThrowsArgumentNullException()
        {
            var ctx = new Context();

            Assert.Throws<ArgumentNullException>(() => SparkCode.Data.XmlToJson.Convert(ctx, string.Empty));
        }
    }
}