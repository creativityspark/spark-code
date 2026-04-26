using Microsoft.Xrm.Sdk;
using System;
using Xunit;

namespace SparkCode.API.Tests.Text
{
    public class ParseTests
    {
        [Fact]
        public void Parse_ValidIntegerString_Returns_Integer()
        {
            var service = new Context().Service;
            var text = "12345";
            var output = service.Execute(new OrganizationRequest("csp_Text_Parse")
            {
                Parameters = new ParameterCollection
                {
                    { "Text", text }
                }
            });
            var integer = (int)output["Integer"];
            Assert.Equal(12345, integer);
        }

        [Fact]
        public void Parse_InvalidIntegerString_Throws_FormatException()
        {
            var service = new Context().Service;
            var text = "abc";
            Assert.ThrowsAny<Exception>(() =>
            {
                service.Execute(new OrganizationRequest("csp_Text_Parse")
                {
                    Parameters = new ParameterCollection
                    {
                        { "Text", text }
                    }
                });
            });
        }
    }
}
