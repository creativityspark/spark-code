using SparkCode.Other;
using System;
using Xunit;

namespace SparkCode.Tests.Other
{
    public class RunCSharpTests
    {
        private static RunCSharpResult Run(string code, string input = null, string types = null,
            string inputType = null, string outputType = null, string references = null, string usings = null)
        {
            return RunCSharp.Execute(code, input, types, inputType, outputType, references, usings);
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData(" ", "\t")]
        public void Execute_DefaultTypes_PreservesStringOutput(string inputType, string outputType)
        {
            var result = Run("output = \"hello world\";", inputType: inputType, outputType: outputType);
            Assert.Equal("hello world", result.Outputs);
            Assert.Null(result.Logs);
            Assert.Empty(result.CompilerErrors);
            Assert.Empty(result.RuntimeErrors);
        }

        [Theory]
        [InlineData(null, "int", "output = Convert.ToInt32(input) * 2;")]
        [InlineData("int", null, "output = (input * 2).ToString();")]
        public void Execute_NativeTypes_PreservesConversion(string inputType, string outputType, string code)
        {
            Assert.Equal("42", Run(code, "21", inputType: inputType, outputType: outputType).Outputs);
        }

        [Fact]
        public void Execute_CustomTypesAndLogs_PreservesOutput()
        {
            var result = Run("output = Helper.Greet(input); Log(\"second\");", "Cris",
                "class Helper { public static string Greet(string name) { CSharpRunner.Log(\"greeting \" + name); return \"Hi \" + name; } }");
            Assert.Equal("Hi Cris", result.Outputs);
            Assert.Equal("greeting Cris\nsecond", result.Logs);
        }

        [Theory]
        [InlineData("\n")]
        [InlineData("\r\n")]
        public void Execute_CompilerErrors_MapCodeLineAndColumn(string newline)
        {
            var result = Run("// first line" + newline + "output = missing;");
            var error = Assert.Single(result.CompilerErrors);
            Assert.Equal("Code", error.Section);
            Assert.Equal(2, error.Line);
            Assert.Equal(10, error.Column);
            Assert.Equal("CS0103", error.Code);
            Assert.Null(result.Outputs);
            Assert.Null(result.Logs);
            Assert.Empty(result.RuntimeErrors);
        }

        [Fact]
        public void Execute_CompilerErrors_MapTypesAndUsingStatements()
        {
            var result = Run("output = missing;",
                types: "// first\nclass Model { public Missing Value; }",
                usings: "// first\nusing MissingNamespace;");
            Assert.Contains(result.CompilerErrors, error => error.Section == "Types" && error.Line == 2 && error.Code == "CS0246");
            Assert.Contains(result.CompilerErrors, error => error.Section == "UsingStatements" && error.Line == 2 && error.Code == "CS0246");
        }

        [Fact]
        public void Execute_Warnings_DoNotPreventSuccess()
        {
            var result = Run("int unused; output = \"ok\";");
            Assert.Equal("ok", result.Outputs);
            Assert.Empty(result.CompilerErrors);
        }

        [Theory]
        [InlineData("output = \"ok\"")]
        [InlineData("if (true) { output = \"ok\";")]
        public void Execute_IncompleteCode_ReturnsCompilerDiagnostics(string code)
        {
            var result = Run(code);
            Assert.NotEmpty(result.CompilerErrors);
            Assert.Null(result.Outputs);
        }

        [Fact]
        public void Execute_MissingBrace_DoesNotInventGeneratedLocation()
        {
            var result = Run("if (true) { output = \"ok\";");
            Assert.Contains(result.CompilerErrors, error =>
                error.Code == "CS1513" && error.Section == null && error.Line == null && error.Column == null);
        }

        [Fact]
        public void Execute_TypedInputAndOutput_PreservesJson()
        {
            var result = Run("output = new Model { Value = input.Value * 2 };", "{\"Value\":21}",
                "class Model { public int Value { get; set; } }", "Model", "Model");
            Assert.Equal("{\"Value\":42}", result.Outputs);
            Assert.Empty(result.CompilerErrors);
            Assert.Empty(result.RuntimeErrors);
        }

        [Fact]
        public void Execute_MissingReference_RemainsConfigurationFault()
        {
            Assert.Throws<InvalidOperationException>(() => Run("output = \"ok\";", references: "missing-assembly.dll"));
        }

        [Fact]
        public void Execute_UnresolvedTypeWithDefinitions_RemainsConfigurationFault()
        {
            Assert.Throws<InvalidOperationException>(() => Run("output = \"ok\";",
                types: "class Model { }", inputType: "Missing"));
        }

        [Fact]
        public void Execute_RuntimeError_PreservesAllLogs()
        {
            var result = Run("Log(\"one\");\nLog(\"two\");\nthrow new InvalidOperationException(\"boom\");");
            var error = Assert.Single(result.RuntimeErrors);
            Assert.Equal("Code", error.Section);
            Assert.True(error.Line == null || error.Line == 3);
            Assert.Equal(typeof(InvalidOperationException).FullName, error.ExceptionType);
            Assert.Equal("boom", error.Message);
            Assert.Contains("Run", error.StackTrace);
            Assert.Equal("one\ntwo", result.Logs);
            Assert.Null(result.Outputs);
            Assert.Empty(result.CompilerErrors);
        }

        [Fact]
        public void Execute_RuntimeErrorInTypes_MapsSection()
        {
            var result = Run("output = Helper.Fail();",
                types: "class Helper {\npublic static string Fail() {\nCSharpRunner.Log(\"before\"); throw new InvalidOperationException(\"boom\");\n}\n}");
            var error = Assert.Single(result.RuntimeErrors);
            Assert.Equal("Types", error.Section);
            Assert.True(error.Line == null || error.Line == 3);
            Assert.Equal("before", result.Logs);
        }

        [Theory]
        [InlineData("not-json")]
        [InlineData("{\"Value\":\"invalid\"}")]
        public void Execute_TypedInputFailure_ReturnsRuntimeErrorWithoutRunning(string input)
        {
            var result = Run("Log(\"must not run\"); output = \"ok\";", input,
                "class Model { public int Value { get; set; } }", "Model");
            var error = Assert.Single(result.RuntimeErrors);
            Assert.Equal("InputParameters", error.Section);
            Assert.NotEmpty(error.ExceptionType);
            Assert.Null(result.Logs);
            Assert.Null(result.Outputs);
            Assert.Empty(result.CompilerErrors);
        }

        [Theory]
        [InlineData("bad")]
        [InlineData(null)]
        [InlineData("")]
        public void Execute_NativeInputFailure_ReturnsRuntimeError(string input)
        {
            var result = Run("Log(\"must not run\"); output = \"ok\";", input, inputType: "int");
            Assert.Equal("InputParameters", Assert.Single(result.RuntimeErrors).Section);
            Assert.Null(result.Outputs);
            Assert.Null(result.Logs);
        }

        [Fact]
        public void Execute_InvalidTypeConfiguration_RemainsFault()
        {
            var exception = Assert.ThrowsAny<Exception>(() => Run("output = \"ok\";", inputType: "Missing"));
            Assert.Contains("Type resolution failed", exception.Message);
        }

        [Fact]
        public void Execute_OutputSerializationFailure_ReturnsRuntimeErrorAndLogs()
        {
            var result = Run("output = new Model(); output.Self = output; Log(\"complete\");",
                types: "class Model { public Model Self { get; set; } }", outputType: "Model");
            var error = Assert.Single(result.RuntimeErrors);
            Assert.Equal("Code", error.Section);
            Assert.Null(error.Line);
            Assert.Null(result.Outputs);
            Assert.Equal("complete", result.Logs);
        }

        [Fact]
        public void Execute_OutputGetterFailure_PreservesSerializationLogs()
        {
            var result = Run("output = new Model(); Log(\"execute\");",
                types: "class Model { public int Value { get { CSharpRunner.Log(\"serialize\"); throw new InvalidOperationException(\"getter\"); } } }",
                outputType: "Model");
            Assert.Single(result.RuntimeErrors);
            Assert.Null(result.Outputs);
            Assert.Equal("execute\nserialize", result.Logs);
        }

        [Fact]
        public void Execute_InputSetterFailure_PreservesConversionLogs()
        {
            var result = Run("Log(\"must not run\"); output = \"ok\";", "{\"Value\":1}",
                "class Model { public int Value { set { CSharpRunner.Log(\"convert\"); throw new InvalidOperationException(\"setter\"); } } }",
                inputType: "Model");
            Assert.Equal("InputParameters", Assert.Single(result.RuntimeErrors).Section);
            Assert.Null(result.Outputs);
            Assert.Equal("convert", result.Logs);
        }
    }
}
