using SparkCode.Other;
using Xunit;

namespace SparkCode.Tests.Other
{
    public class RunCSharpTests
    {
        [Fact]
        public void Execute_NullTypeNames_DefaultsToString()
        {
            var output = RunCSharp.Execute(
                "output = \"hello world\";",
                null,
                null,
                null,
                null,
                null,
                null);

            Assert.Equal("hello world", output);
        }

        [Fact]
        public void Execute_WhitespaceTypeNames_DefaultsToString()
        {
            var output = RunCSharp.Execute(
                "output = \"hello world\";",
                null,
                null,
                " ",
                "\t",
                null,
                null);

            Assert.Equal("hello world", output);
        }

        [Fact]
        public void Execute_NullInputTypeName_DefaultsToString()
        {
            var output = RunCSharp.Execute(
                "output = Convert.ToInt32(input) * 2;",
                "21",
                null,
                null,
                "int",
                null,
                null);

            Assert.Equal("42", output);
        }

        [Fact]
        public void Execute_NullOutputTypeName_DefaultsToString()
        {
            var output = RunCSharp.Execute(
                "output = (input * 2).ToString();",
                "21",
                null,
                "int",
                null,
                null,
                null);

            Assert.Equal("42", output);
        }
    }
}
