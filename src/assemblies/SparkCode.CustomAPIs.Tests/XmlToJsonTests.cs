using SparkCode.CustomAPIs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SparkCode.CustomAPIs.Tests
{
    public class XmlToJsonTests
    {

        [Fact]
        public void Convert_ValidXml_ReturnsJson()
        {
            var ctx = new Context();

            var xmlToJson = new XmlToJson();
            string xml = "<root><element>value</element></root>";
            string expectedJson = "{\"root\":{\"element\":\"value\"}}";
            string json = xmlToJson.Convert(ctx, xml);
            Assert.Equal(expectedJson, json);
        }


        [Fact]
        public void Convert_InvalidXml_ThrowsException()
        {
            var ctx = new Context();
            var xmlToJson = new XmlToJson();
            string invalidXml = "<root><element>value</element>";
            Assert.Throws<System.Xml.XmlException>(() => xmlToJson.Convert(ctx, invalidXml));
        }

        [Fact]
        public void Convert_EmptyXml_ReturnsEmptyJson()
        {
            var ctx = new Context();
            var xmlToJson = new XmlToJson();
            string xml = "<root></root>";
            string expectedJson = "{\"root\":\"\"}";
            string json = xmlToJson.Convert(ctx, xml);
            Assert.Equal(expectedJson, json);
        }

        [Fact]
        public void Convert_Complex_Xml()
        {
            var ctx = new Context();
            var xmlToJson = new XmlToJson();

            string xml = @"<?xml version='1.0' standalone='no'?>
                            <root>
                              <person id='1'>
                              <name>Alan</name>
                              <url>http://www.google.com</url>
                              </person>
                              <person id='2'>
                              <name>Louis</name>
                              <url>http://www.yahoo.com</url>
                              </person>
                            </root>";

            string expectedJson = @"{""?xml"":{""@version"":""1.0"",""@standalone"":""no""},""root"":{""person"":[{""@id"":""1"",""name"":""Alan"",""url"":""http://www.google.com""},{""@id"":""2"",""name"":""Louis"",""url"":""http://www.yahoo.com""}]}}";

            string json = xmlToJson.Convert(ctx, xml);
            Assert.Equal(expectedJson, json);
        }

    }
}
