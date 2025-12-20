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
            var output = service.Execute(new OrganizationRequest("csp_Text.ParseURL")
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
        }


        [Fact]
        public void ParseURL_ValidUrl_Returns_Parsed_Components_Json()
        {
            var service = Context.GetService();
            var url = "https://example.com:8080/path/to/resource?query=param#fragment";
            var output = service.Execute(new OrganizationRequest("csp_Text.ParseURLJson")
            {
                Parameters = new ParameterCollection
                {
                    { "Url", url }
                }
            });
            // Test only that there are values returned, not the actual values
            var results = (string)output["ResultsJson"];
            Assert.NotNull(results);
        }
    }
}
