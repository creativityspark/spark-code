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

        [Fact]
        public void Execute_Log_ReturnsLogsInOrder()
        {
            string logs;
            var output = RunCSharp.Execute(
                "output = \"Hello \" + input; Log(output); Log(\"second\");",
                "Cris",
                null,
                null,
                null,
                null,
                null,
                out logs);

            Assert.Equal("Hello Cris", output);
            Assert.Equal("Hello Cris\nsecond", logs);
        }

        [Fact]
        public void Execute_NoLog_ReturnsNullLogs()
        {
            string logs;
            RunCSharp.Execute("output = \"x\";", null, null, null, null, null, null, out logs);

            Assert.Null(logs);
        }

        [Fact]
        public void Execute_LogFromCustomType_IsCollected()
        {
            string logs;
            var output = RunCSharp.Execute(
                "output = Helper.Greet((string)input);",
                "Cris",
                "class Helper { public static string Greet(string name) { CSharpRunner.Log(\"greeting \" + name); return \"Hi \" + name; } }",
                null,
                null,
                null,
                null,
                out logs);

            Assert.Equal("Hi Cris", output);
            Assert.Equal("greeting Cris", logs);
        }

        [Fact]
        public void Execute_RuntimeError_IncludesLogsInException()
        {
            string logs;
            var exception = Assert.ThrowsAny<System.Exception>(() => RunCSharp.Execute(
                "Log(\"before failure\"); throw new InvalidOperationException(\"boom\");",
                null,
                null,
                null,
                null,
                null,
                null,
                out logs));

            Assert.Contains("boom", exception.Message);
            Assert.Contains("before failure", exception.Message);
        }
    }
}
