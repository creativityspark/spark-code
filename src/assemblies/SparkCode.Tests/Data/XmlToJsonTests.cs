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
            string xml = "<root><element>value</element></root>";
            string expectedJson = "{\"root\":{\"element\":\"value\"}}";

            string json = SparkCode.Data.XmlToJson.Convert(xml);
            Assert.Equal(expectedJson, json);
        }

        [Fact]
        public void Convert_InvalidXml_ThrowsException()
        {
            string invalidXml = "<root><element>value</element>";

            Assert.Throws<XmlException>(() => SparkCode.Data.XmlToJson.Convert(invalidXml));
        }

        [Fact]
        public void Convert_EmptyXml_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => SparkCode.Data.XmlToJson.Convert(string.Empty));
        }
    }
}