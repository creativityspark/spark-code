using Microsoft.Xrm.Sdk;
using Newtonsoft.Json.Linq;
using System;
using Xunit;

namespace SparkCode.API.Tests.Other
{
    public class RunCSharpTests
    {
        [Theory]
        [InlineData("output = \"ok\";", "ok", null)]
        [InlineData("output = missing;", null, "CompilerErrors")]
        [InlineData("Log(\"before\"); throw new InvalidOperationException(\"boom\");", null, "RuntimeErrors")]
        public void RunCSharp_ReturnsOutputsAndExpectedDiagnostics(string code, string output, string diagnosticProperty)
        {
            var response = new Context().Service.Execute(new OrganizationRequest("csp_Other_RunCSharp")
            {
                Parameters = new ParameterCollection { { "Code", code } }
            });

            Assert.Equal(output, (string)response["Outputs"]);
            Assert.False(response.Results.Contains("CompileErrors"));
            foreach (var property in new[] { "CompilerErrors", "RuntimeErrors" })
            {
                if (property == diagnosticProperty)
                {
                    Assert.NotEmpty(JArray.Parse(Assert.IsType<string>(response[property])));
                }
                else
                {
                    Assert.True(!response.Results.Contains(property) || response[property] == null,
                        $"Expected no diagnostics in '{property}'.");
                }
            }
        }

        [Fact]
        public void RunCSharp_CompilerLocations_AreSectionRelativeInDataverse()
        {
            var response = new Context().Service.Execute(new OrganizationRequest("csp_Other_RunCSharp")
            {
                Parameters = new ParameterCollection { { "Code", "// first\r\noutput = missing;" } }
            });
            var error = (JObject)Assert.Single(JArray.Parse((string)response["CompilerErrors"]));
            Assert.Equal("Code", (string)error["section"]);
            Assert.Equal(2, (int)error["line"]);
            Assert.Equal(10, (int)error["column"]);
            Assert.Equal("CS0103", (string)error["code"]);
        }

        [Fact]
        public void RunCSharp_RuntimeLocations_AreNullOrUserRelativeInDataverse()
        {
            var response = new Context().Service.Execute(new OrganizationRequest("csp_Other_RunCSharp")
            {
                Parameters = new ParameterCollection
                {
                    { "Code", "Log(\"before\");\nthrow new InvalidOperationException(\"boom\");" }
                }
            });
            var error = (JObject)Assert.Single(JArray.Parse((string)response["RuntimeErrors"]));
            Assert.Equal("Code", (string)error["section"]);
            Assert.True(error["line"].Type == JTokenType.Null || (int)error["line"] == 2);
            Assert.NotNull(error.Property("stackTrace"));
        }

        [Fact]
        public void RunCSharp_ValidCode_Returns_Outputs()
        {
            var service = new Context().Service;
            var code = "output = \"hello world\";";

            var output = service.Execute(new OrganizationRequest("csp_Other_RunCSharp")
            {
                Parameters = new ParameterCollection
                {
                    { "Code", code }
                }
            });

            Assert.True(output.Results.Contains("Outputs"), "Expected output parameter 'Outputs' was not returned.");
            Assert.Equal("hello world", (string)output["Outputs"]);
        }

        [Fact]
        public void RunCSharp_WithInputParameters_Returns_RuntimeValue()
        {
            var service = new Context().Service;
            var code = "output = \"hello \"+input;";
            var inputParameters = "Cris";

            var output = service.Execute(new OrganizationRequest("csp_Other_RunCSharp")
            {
                Parameters = new ParameterCollection
                {
                    { "Code", code },
                    { "InputParameters", inputParameters }
                }
            });

            Assert.Equal("hello Cris", (string)output["Outputs"]);
        }

        [Fact]
        public void RunCSharp_WithLog_Returns_Logs()
        {
            var service = new Context().Service;
            var code = "output = \"Hello \" + input; Log(output); Log(\"done\");";

            var output = service.Execute(new OrganizationRequest("csp_Other_RunCSharp")
            {
                Parameters = new ParameterCollection
                {
                    { "Code", code },
                    { "InputParameters", "Cris" }
                }
            });

            Assert.Equal("Hello Cris", (string)output["Outputs"]);
            Assert.True(output.Results.Contains("Logs"), "Expected output parameter 'Logs' was not returned.");
            Assert.Equal("Hello Cris\ndone", (string)output["Logs"]);
        }

        [Fact]
        public void RunCSharp_RuntimeError_Returns_DiagnosticsAndLogs()
        {
            var service = new Context().Service;
            var code = "Log(\"before failure\"); throw new InvalidOperationException(\"boom\");";

            var response = service.Execute(new OrganizationRequest("csp_Other_RunCSharp")
                {
                    Parameters = new ParameterCollection
                    {
                        { "Code", code }
                    }
                });

            var error = (JObject)Assert.Single(JArray.Parse((string)response["RuntimeErrors"]));
            Assert.Equal("boom", (string)error["message"]);
            Assert.Equal("Code", (string)error["section"]);
            Assert.Equal("System.InvalidOperationException", (string)error["exceptionType"]);
            Assert.Equal("before failure", (string)response["Logs"]);
            Assert.True(!response.Results.Contains("Outputs") || response["Outputs"] == null);
            Assert.True(!response.Results.Contains("CompilerErrors") || response["CompilerErrors"] == null);
        }

        [Fact]
        public void RunCSharp_WithUsingStatementsAndReferencedAssemblies_Returns_TransformedValue()
        {
            var service = new Context().Service;
            var code = "output = Regex.Replace((string)input, \"[^a-zA-Z]\", \"\");";

            var output = service.Execute(new OrganizationRequest("csp_Other_RunCSharp")
            {
                Parameters = new ParameterCollection
                {
                    { "Code", code },
                    { "InputParameters", "a1b2-c3" },
                    { "UsingStatements", "using System.Text.RegularExpressions;" },
                    { "ReferencedAssemblies", "System.dll,System.Core.dll" }
                }
            });

            Assert.Equal("abc", (string)output["Outputs"]);
        }

        [Fact]
        public void RunCSharp_InvalidCode_Returns_CompileDiagnostics()
        {
            var service = new Context().Service;
            var code = "output = ;";

            var response = service.Execute(new OrganizationRequest("csp_Other_RunCSharp")
                {
                    Parameters = new ParameterCollection
                    {
                        { "Code", code }
                    }
                });

            var errors = JArray.Parse((string)response["CompilerErrors"]);
            Assert.NotEmpty(errors);
            Assert.All(errors, error =>
            {
                Assert.Equal("Code", (string)error["section"]);
                Assert.True((int)error["line"] > 0);
                Assert.True((int)error["column"] > 0);
                Assert.StartsWith("CS", (string)error["code"]);
            });
            Assert.True(!response.Results.Contains("Outputs") || response["Outputs"] == null);
            Assert.True(!response.Results.Contains("RuntimeErrors") || response["RuntimeErrors"] == null);
        }

        [Fact]
        public void RunCSharp_TypedInputAndOutput_Returns_SerializedOutputJson()
        {
            var service = new Context().Service;
            var types = @"class InputType { public int MyProperty { get; set; } public string MyValue { get; set; } }
class OutputType { public int MyProperty { get; set; } }";
            var code = "var model = (InputType)input; output = new OutputType { MyProperty = model.MyProperty };";
            var inputParameters = "{\"MyProperty\":123,\"MyValue\":\"abc\"}";

            var output = service.Execute(new OrganizationRequest("csp_Other_RunCSharp")
            {
                Parameters = new ParameterCollection
                {
                    { "Code", code },
                    { "Types", types },
                    { "InputParameters", inputParameters },
                    { "InputTypeName", "InputType" },
                    { "OutputTypeName", "OutputType" }
                }
            });

            var payload = JObject.Parse((string)output["Outputs"]);
            Assert.Equal(123, (int)payload["MyProperty"]);
        }

        [Fact]
        public void RunCSharp_NativeInputAndOutputType_Returns_ConvertedString()
        {
            var service = new Context().Service;
            var code = "var value = (int)input; output = value * 2;";

            var output = service.Execute(new OrganizationRequest("csp_Other_RunCSharp")
            {
                Parameters = new ParameterCollection
                {
                    { "Code", code },
                    { "InputParameters", "21" },
                    { "InputTypeName", "int" },
                    { "OutputTypeName", "int" }
                }
            });

            Assert.Equal("42", (string)output["Outputs"]);
        }

        [Fact]
        public void RunCSharp_CustomTypeWithoutTypes_Throws_ClearError()
        {
            var service = new Context().Service;
            var code = "output = input;";

            var exception = Assert.ThrowsAny<Exception>(() =>
            {
                service.Execute(new OrganizationRequest("csp_Other_RunCSharp")
                {
                    Parameters = new ParameterCollection
                    {
                        { "Code", code },
                        { "InputParameters", "{}" },
                        { "InputTypeName", "InputType" }
                    }
                });
            });

            Assert.Contains("Type resolution failed for 'InputType'", exception.Message);
        }

        [Fact]
        public void RunCSharp_MalformedJsonForCustomInput_Returns_RuntimeDiagnostics()
        {
            var service = new Context().Service;
            var types = "class InputType { public int MyProperty { get; set; } }";
            var code = "output = \"ok\";";

            var response = service.Execute(new OrganizationRequest("csp_Other_RunCSharp")
                {
                    Parameters = new ParameterCollection
                    {
                        { "Code", code },
                        { "Types", types },
                        { "InputParameters", "not-json" },
                        { "InputTypeName", "InputType" }
                    }
                });

            var error = (JObject)Assert.Single(JArray.Parse((string)response["RuntimeErrors"]));
            Assert.Equal("InputParameters", (string)error["section"]);
            Assert.Contains("Json", (string)error["exceptionType"]);
            Assert.True(!response.Results.Contains("Outputs") || response["Outputs"] == null);
            Assert.True(!response.Results.Contains("Logs") || response["Logs"] == null);
        }

        [Fact]
        public void RunCSharp_OutputSerializationError_PreservesLogs()
        {
            var response = new Context().Service.Execute(new OrganizationRequest("csp_Other_RunCSharp")
            {
                Parameters = new ParameterCollection
                {
                    { "Code", "output = new Model(); output.Self = output; Log(\"complete\");" },
                    { "Types", "class Model { public Model Self { get; set; } }" },
                    { "OutputTypeName", "Model" }
                }
            });
            var error = (JObject)Assert.Single(JArray.Parse((string)response["RuntimeErrors"]));
            Assert.Equal(JTokenType.Null, error["line"].Type);
            Assert.Equal("Code", (string)error["section"]);
            Assert.Equal("complete", (string)response["Logs"]);
            Assert.True(!response.Results.Contains("Outputs") || response["Outputs"] == null);
        }
    }
}
