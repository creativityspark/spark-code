using Microsoft.Xrm.Sdk;
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
    }
}
