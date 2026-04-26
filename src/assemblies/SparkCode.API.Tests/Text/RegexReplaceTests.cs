using Microsoft.Xrm.Sdk;
using System;
using Xunit;

namespace SparkCode.API.Tests.Text
{
    public class RegexReplaceTests
    {
        [Fact]
        public void RegexReplace_ValidInput_Returns_Replaced_Text()
        {
            var service = new Context().Service;

            var output = service.Execute(new OrganizationRequest("csp_Text_RegexReplace")
            {
                Parameters = new ParameterCollection
                {
                    { "Input", "Item-123-ABC" },
                    { "Pattern", "\\d" },
                    { "Replacement", "X" }
                }
            });

            var results = (string)output["Results"];
            Assert.Equal("Item-XXX-ABC", results);
        }

        [Fact]
        public void RegexReplace_NoMatch_Returns_Original_Text()
        {
            var service = new Context().Service;

            var output = service.Execute(new OrganizationRequest("csp_Text_RegexReplace")
            {
                Parameters = new ParameterCollection
                {
                    { "Input", "No digits here" },
                    { "Pattern", "\\d+" },
                    { "Replacement", "0" }
                }
            });

            var results = (string)output["Results"];
            Assert.Equal("No digits here", results);
        }

        [Fact]
        public void RegexReplace_InvalidPattern_Throws_Exception()
        {
            var service = new Context().Service;

            Assert.ThrowsAny<Exception>(() =>
            {
                service.Execute(new OrganizationRequest("csp_Text_RegexReplace")
                {
                    Parameters = new ParameterCollection
                    {
                        { "Input", "ABC" },
                        { "Pattern", "(" },
                        { "Replacement", "X" }
                    }
                });
            });
        }
    }
}