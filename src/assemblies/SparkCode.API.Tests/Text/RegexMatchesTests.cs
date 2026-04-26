using Microsoft.Xrm.Sdk;
using System;
using Xunit;

namespace SparkCode.API.Tests.Text
{
    public class RegexMatchesTests
    {
        [Fact]
        public void RegexMatches_ValidInput_Returns_Matches_EntityCollection()
        {
            var service = new Context().Service;

            var output = service.Execute(new OrganizationRequest("csp_Text_RegexMatches")
            {
                Parameters = new ParameterCollection
                {
                    { "Input", "A1 B22 C333" },
                    { "Pattern", "\\d+" }
                }
            });

            var results = (EntityCollection)output["Results"];

            Assert.NotNull(results);
            Assert.Equal(3, results.Entities.Count);
            Assert.Equal("1", (string)results.Entities[0]["value"]);
            Assert.Equal(1, (int)results.Entities[0]["index"]);
            Assert.Equal(1, (int)results.Entities[0]["length"]);
            Assert.Equal("22", (string)results.Entities[1]["value"]);
            Assert.Equal("333", (string)results.Entities[2]["value"]);
        }

        [Fact]
        public void RegexMatchesJson_ValidInput_Returns_ResultsJson()
        {
            var service = new Context().Service;

            var output = service.Execute(new OrganizationRequest("csp_Text_RegexMatchesJson")
            {
                Parameters = new ParameterCollection
                {
                    { "Input", "A1 B22 C333" },
                    { "Pattern", "\\d+" }
                }
            });

            var resultsJson = (string)output["ResultsJson"];
            var parsedJson = System.Text.Json.JsonDocument.Parse(resultsJson);

            Assert.Equal(3, parsedJson.RootElement.GetArrayLength());
            Assert.Equal("1", parsedJson.RootElement[0].GetProperty("value").GetString());
            Assert.Equal(1, parsedJson.RootElement[0].GetProperty("index").GetInt32());
            Assert.Equal(1, parsedJson.RootElement[0].GetProperty("length").GetInt32());
        }

        [Fact]
        public void RegexMatches_InvalidPattern_Throws_Exception()
        {
            var service = new Context().Service;

            Assert.ThrowsAny<Exception>(() =>
            {
                service.Execute(new OrganizationRequest("csp_Text_RegexMatches")
                {
                    Parameters = new ParameterCollection
                    {
                        { "Input", "A1" },
                        { "Pattern", "(" }
                    }
                });
            });
        }
    }
}