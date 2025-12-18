using Microsoft.Xrm.Sdk;
using Xunit;

namespace SparkCode.API.Tests.Text
{
    public class ParseURLTests
    {
        [Fact]
        public void ParseURL_ValidUrl_Returns_Parsed_Components()
        {
            var service = Context.GetService();
            var url = "https://example.com:8080/path/to/resource?query=param#fragment";
            var result = service.Execute(new OrganizationRequest("csp_Text.ParseURL")
            {
                Parameters = new ParameterCollection
                {
                    { "url", url }
                }
            });
            Assert.Equal("https", result["scheme"]);
            Assert.Equal("example.com", result["host"]);
            Assert.Equal(8080, result["port"]);
            Assert.Equal("/path/to/resource", result["path"]);
            Assert.Equal("query=param", result["query"]);
            Assert.Equal("fragment", result["fragment"]);

        }
    }
}
