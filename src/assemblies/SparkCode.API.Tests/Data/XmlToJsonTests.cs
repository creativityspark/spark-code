using Microsoft.Xrm.Sdk;
using System;
using Xunit;

namespace SparkCode.API.Tests.Data
{
    public class XmlToJsonTests
    {
        [Fact]
        public void XmlToJson_ValidXml_Returns_Json()
        {
            var service = Context.GetService();
            var xml = "<root><element>value</element></root>";

            var output = service.Execute(new OrganizationRequest("csp_Data_XmlToJson")
            {
                Parameters = new ParameterCollection
                {
                    { "Xml", xml }
                }
            });

            var results = (string)output["Results"];
            Assert.Equal("{\"root\":{\"element\":\"value\"}}", results);
        }

        [Fact]
        public void XmlToJson_InvalidXml_Throws_Exception()
        {
            var service = Context.GetService();
            var invalidXml = "<root><element>value</element>";

            Assert.ThrowsAny<Exception>(() =>
            {
                service.Execute(new OrganizationRequest("csp_Data_XmlToJson")
                {
                    Parameters = new ParameterCollection
                    {
                        { "Xml", invalidXml }
                    }
                });
            });
        }
    }
}