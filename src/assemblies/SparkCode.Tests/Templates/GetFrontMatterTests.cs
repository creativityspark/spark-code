using Microsoft.Xrm.Sdk;
using Xunit;

namespace SparkCode.Tests.Templates
{
    public class GetFrontMatterTests
    {
        [Fact]
        public void Parse_WithFrontMatter_Returns_FrontMatter_And_Body()
        {
            var inputText = "---\ntitle: Hello\nauthor: Cris\n---\n# Body";

            var results = SparkCode.Templates.GetFrontMatter.Parse(inputText);
            var frontMatter = (Entity)results["frontMatter"];
            var body = (string)results["body"];

            Assert.Equal("Hello", (string)frontMatter["title"]);
            Assert.Equal("Cris", (string)frontMatter["author"]);
            Assert.Equal("# Body", body);
        }

        [Fact]
        public void Parse_WithoutFrontMatter_Returns_EmptyFrontMatter_And_OriginalBody()
        {
            var inputText = "# Body";

            var results = SparkCode.Templates.GetFrontMatter.Parse(inputText);
            var frontMatter = (Entity)results["frontMatter"];
            var body = (string)results["body"];

            Assert.Empty(frontMatter.Attributes);
            Assert.Equal(inputText, body);
        }

        [Fact]
        public void Parse_WithQuotedValues_Removes_Quotes()
        {
            var inputText = "---\ntitle: \"Hello World\"\n---\nBody";

            var results = SparkCode.Templates.GetFrontMatter.Parse(inputText);
            var frontMatter = (Entity)results["frontMatter"];

            Assert.Equal("Hello World", (string)frontMatter["title"]);
        }
    }
}