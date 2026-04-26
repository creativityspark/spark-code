using SparkCode.Text;
using Xunit;

namespace SparkCode.Tests.Text
{
    public class ReplaceParamsTests
    {
        [Fact]
        public void ReplaceParams_AllParams_ReplacesAllPlaceholders()
        {
            string text = "{{param1}}-{{param2}}-{{param3}}-{{param4}}-{{param5}}-{{param6}}-{{param7}}-{{param8}}-{{param9}}";
            string[] parameters = new[] { "A", "B", "C", "D", "E", "F", "G", "H", "I" };
            var result = ReplaceParams.Replace(text, parameters);
            Assert.Equal("A-B-C-D-E-F-G-H-I", result);
        }

        [Fact]
        public void ReplaceParams_SomeParamsNull_OnlyNonNullReplaced()
        {
            string text = "{{param1}}-{{param2}}-{{param3}}-{{param4}}-{{param5}}";
            string[] parameters = new[] { "A", null, "C", null, "E", null, null, null, null };
            var result = SparkCode.Text.ReplaceParams.Replace(text, parameters);
            Assert.Equal("A-{{param2}}-C-{{param4}}-E", result);
        }

        [Fact]
        public void ReplaceParams_EmptyParams_NoReplacement()
        {
            string text = "{{param1}}-{{param2}}-{{param3}}";
            string[] parameters = new string[9];
            var result = SparkCode.Text.ReplaceParams.Replace(text, parameters);
            Assert.Equal("{{param1}}-{{param2}}-{{param3}}", result);
        }
    }
}
