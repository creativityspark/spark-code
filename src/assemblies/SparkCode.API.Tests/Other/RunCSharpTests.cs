using Microsoft.Xrm.Sdk;
using Newtonsoft.Json.Linq;
using System;
using Xunit;

namespace SparkCode.API.Tests.Other
{
    public class RunCSharpTests
    {
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
        public void RunCSharp_InvalidCode_Throws_Exception()
        {
            var service = new Context().Service;
            var code = "output = ;";

            var exception = Assert.ThrowsAny<Exception>(() =>
            {
                service.Execute(new OrganizationRequest("csp_Other_RunCSharp")
                {
                    Parameters = new ParameterCollection
                    {
                        { "Code", code }
                    }
                });
            });

            Assert.Contains("Compiler Errors:", exception.Message);
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
        public void RunCSharp_MalformedJsonForCustomInput_Throws_ClearError()
        {
            var service = new Context().Service;
            var types = "class InputType { public int MyProperty { get; set; } }";
            var code = "output = \"ok\";";

            var exception = Assert.ThrowsAny<Exception>(() =>
            {
                service.Execute(new OrganizationRequest("csp_Other_RunCSharp")
                {
                    Parameters = new ParameterCollection
                    {
                        { "Code", code },
                        { "Types", types },
                        { "InputParameters", "not-json" },
                        { "InputTypeName", "InputType" }
                    }
                });
            });

            Assert.Contains("Input conversion failed for type 'InputType'", exception.Message);
        }
    }
}
