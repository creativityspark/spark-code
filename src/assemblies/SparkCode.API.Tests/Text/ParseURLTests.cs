using Microsoft.Xrm.Sdk;
using System;
using Xunit;

namespace SparkCode.API.Tests.Text
{
    public class ParseURLTests
    {
        [Fact]
        public void ParseURL_ValidUrl_Returns_Parsed_Components()
        {
            var service = new Context().Service;
            var expectedId = Guid.NewGuid().ToString();
            var url = $"https://example.com:8080/path/to/resource?query=param&id={expectedId}&etn=account#fragment";
            var output = service.Execute(new OrganizationRequest("csp_Text_ParseURL")
            {
                Parameters = new ParameterCollection
                {
                    { "Url", url }
                }
            });
            // Test only that there are values returned, not the actual values
            var results = (Entity)output["Results"];
            var query = (Entity)results["query"];
            Assert.NotNull(results);
            Assert.NotNull(query);
            Assert.Equal(expectedId, (string)query["id"]);
            Assert.Equal("account", (string)query["etn"]);
            Assert.Equal(expectedId, (string)output["Id"]);
            Assert.Equal("account", (string)output["Etn"]);
        }

        [Fact]
        public void ParseURL_WithoutIdAndEtn_DoesNotReturn_ThoseOutputs()
        {
            var service = new Context().Service;
            var url = "https://example.com:8080/path/to/resource?query=param#fragment";
            var output = service.Execute(new OrganizationRequest("csp_Text_ParseURL")
            {
                Parameters = new ParameterCollection
                {
                    { "Url", url }
                }
            });
            var results = (Entity)output["Results"];
            var query = (Entity)results["query"];
            Assert.NotNull(results);
            Assert.NotNull(query);
            Assert.False(query.Attributes.Contains("id"));
            Assert.False(query.Attributes.Contains("etn"));
            Assert.False(results.Contains("Id"));
            Assert.False(results.Contains("Etn"));
        }

        [Fact]
        public void ParseURL_ValidUrl_Returns_Parsed_Components_Json()
        {
            var service = new Context().Service;
            var url = "https://example.com:8080/path/to/resource?query=param#fragment";
            var output = service.Execute(new OrganizationRequest("csp_Text_ParseURLJson")
            {
                Parameters = new ParameterCollection
                {
                    { "Url", url }
                }
            });
            var results = (string)output["ResultsJson"];
            var parsedJson = System.Text.Json.JsonDocument.Parse(results);
            Assert.Equal("https", parsedJson.RootElement.GetProperty("scheme").GetString());
            Assert.Equal("example.com", parsedJson.RootElement.GetProperty("host").GetString());
            Assert.Equal(8080, parsedJson.RootElement.GetProperty("port").GetInt32());
            Assert.Equal("/path/to/resource", parsedJson.RootElement.GetProperty("absolutePath").GetString());
            Assert.Equal("fragment", parsedJson.RootElement.GetProperty("fragment").GetString());
            var query = parsedJson.RootElement.GetProperty("query");
            Assert.Equal("param", query.GetProperty("query").GetString());
        }

        [Fact]
        public void ParseURL_InvalidUrl_Throws_Exception()
        {
            var service = new Context().Service;
            var invalidUrl = "not_a_valid_url";
            Assert.ThrowsAny<Exception>(() =>
            {
                service.Execute(new OrganizationRequest("csp_Text_ParseURL")
                {
                    Parameters = new ParameterCollection
                    {
                        { "Url", invalidUrl }
                    }
                });
            });
        }

        [Fact]
        public void ParseURLJson_InvalidUrl_Throws_Exception()
        {
            var service = new Context().Service;
            var invalidUrl = "not_a_valid_url";
            Assert.ThrowsAny<Exception>(() =>
            {
                service.Execute(new OrganizationRequest("csp_Text_ParseURLJson")
                {
                    Parameters = new ParameterCollection
                    {
                        { "Url", invalidUrl }
                    }
                });
            });
        }
    }
}
