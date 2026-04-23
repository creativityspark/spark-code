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

            Assert.True(output.Results.Contains("FrontMatter"), "Expected output parameter 'FrontMatter' was not returned.");
            Assert.True(output.Results.Contains("Body"), "Expected output parameter 'Body' was not returned.");

            var frontMatter = (Entity)output["FrontMatter"];
            var body = (string)output["Body"];

            Assert.Equal("Hello", (string)frontMatter["title"]);
            Assert.Equal("Cris", (string)frontMatter["author"]);
            Assert.Equal("# Body", body);
        }

        [Fact]
        public void GetFrontMatterJson_WithFrontMatter_Returns_FrontMatterJson_And_Body()
        {
            var service = Context.GetService();
            var inputText = "---\ntitle: Hello\nauthor: Cris\n---\n# Body";
            var output = service.Execute(new OrganizationRequest("csp_Templates_GetFrontMatterJson")
            {
                Parameters = new ParameterCollection
                {
                    { "InputText", inputText }
                }
            });

            Assert.True(output.Results.Contains("FrontMatterJson"), "Expected output parameter 'FrontMatterJson' was not returned.");
            Assert.True(output.Results.Contains("Body"), "Expected output parameter 'Body' was not returned.");

            var frontMatterJson = (string)output["FrontMatterJson"];
            var body = (string)output["Body"];
            var parsedJson = JsonDocument.Parse(frontMatterJson);

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

            Assert.True(output.Results.Contains("FrontMatter"), "Expected output parameter 'FrontMatter' was not returned.");
            Assert.True(output.Results.Contains("Body"), "Expected output parameter 'Body' was not returned.");

            var frontMatter = (Entity)output["FrontMatter"];
            var body = (string)output["Body"];

            Assert.Empty(frontMatter.Attributes);
            Assert.Equal(inputText, body);
        }

        [Fact]
        public void GetFrontMatterJson_WithoutFrontMatter_Returns_EmptyFrontMatterJson_And_OriginalBody()
        {
            var service = Context.GetService();
            var inputText = "# Body";
            var output = service.Execute(new OrganizationRequest("csp_Templates_GetFrontMatterJson")
            {
                Parameters = new ParameterCollection
                {
                    { "InputText", inputText }
                }
            });

            Assert.True(output.Results.Contains("FrontMatterJson"), "Expected output parameter 'FrontMatterJson' was not returned.");
            Assert.True(output.Results.Contains("Body"), "Expected output parameter 'Body' was not returned.");

            var frontMatterJson = (string)output["FrontMatterJson"];
            var body = (string)output["Body"];
            var parsedJson = JsonDocument.Parse(frontMatterJson);

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

            Assert.True(output.Results.Contains("FrontMatter"), "Expected output parameter 'FrontMatter' was not returned.");

            var frontMatter = (Entity)output["FrontMatter"];

            Assert.Equal("Hello World", (string)frontMatter["title"]);
        }

        [Fact]
        public void GetFrontMatterJson_WithQuotedValues_Removes_Quotes()
        {
            var service = Context.GetService();
            var inputText = "---\ntitle: \"Hello World\"\n---\nBody";
            var output = service.Execute(new OrganizationRequest("csp_Templates_GetFrontMatterJson")
            {
                Parameters = new ParameterCollection
                {
                    { "InputText", inputText }
                }
            });

            Assert.True(output.Results.Contains("FrontMatterJson"), "Expected output parameter 'FrontMatterJson' was not returned.");

            var frontMatterJson = (string)output["FrontMatterJson"];
            var parsedJson = JsonDocument.Parse(frontMatterJson);

            Assert.Equal("Hello World", parsedJson.RootElement.GetProperty("title").GetString());
        }
    }
}