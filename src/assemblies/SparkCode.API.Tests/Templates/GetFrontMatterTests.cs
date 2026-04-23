using Microsoft.Xrm.Sdk;
using System.Text.Json;
using Xunit;

namespace SparkCode.API.Tests.Templates
{
    public class GetFrontMatterTests
    {
        [Fact]
        public void GetFrontMatter_WithFrontMatter_Returns_FrontMatter_And_Body()
        {
            var service = Context.GetService();
            var inputText = "---\ntitle: Hello\nauthor: Cris\n---\n# Body";
            var output = service.Execute(new OrganizationRequest("csp_Templates_GetFrontMatter")
            {
                Parameters = new ParameterCollection
                {
                    { "InputText", inputText }
                }
            });

            var frontMatter = (string)output["FrontMatter"];
            var body = (string)output["Body"];
            var parsedJson = JsonDocument.Parse(frontMatter);

            Assert.Equal("Hello", parsedJson.RootElement.GetProperty("title").GetString());
            Assert.Equal("Cris", parsedJson.RootElement.GetProperty("author").GetString());
            Assert.Equal("# Body", body);
        }

        [Fact]
        public void GetFrontMatter_WithoutFrontMatter_Returns_EmptyFrontMatter_And_OriginalBody()
        {
            var service = Context.GetService();
            var inputText = "# Body";
            var output = service.Execute(new OrganizationRequest("csp_Templates_GetFrontMatter")
            {
                Parameters = new ParameterCollection
                {
                    { "InputText", inputText }
                }
            });

            var frontMatter = (string)output["FrontMatter"];
            var body = (string)output["Body"];
            var parsedJson = JsonDocument.Parse(frontMatter);

            Assert.Equal("{}", parsedJson.RootElement.GetRawText());
            Assert.Equal(inputText, body);
        }

        [Fact]
        public void GetFrontMatter_WithQuotedValues_Removes_Quotes()
        {
            var service = Context.GetService();
            var inputText = "---\ntitle: \"Hello World\"\n---\nBody";
            var output = service.Execute(new OrganizationRequest("csp_Templates_GetFrontMatter")
            {
                Parameters = new ParameterCollection
                {
                    { "InputText", inputText }
                }
            });

            var frontMatter = (string)output["FrontMatter"];
            var parsedJson = JsonDocument.Parse(frontMatter);

            Assert.Equal("Hello World", parsedJson.RootElement.GetProperty("title").GetString());
        }
    }
}