using Microsoft.Xrm.Sdk;
using Xunit;

namespace SparkCode.API.Tests.Text
{
    public class Base64EncodeTests
    {
        [Fact]
        public void Base64Encode_ValidText_Returns_Base64_String()
        {
            var service = Context.GetService();
            var input = "Hello World";
            var output = service.Execute(new OrganizationRequest("csp_Text_Base64Encode")
            {
                Parameters = new ParameterCollection
                {
                    { "Input", input }
                }
            });
            var result = (string)output["Output"];
            Assert.Equal("SGVsbG8gV29ybGQ=", result);
        }

        [Fact]
        public void Base64Encode_ValidUnicode_Returns_Base64_String()
        {
            var service = Context.GetService();
            var input = "\u00A1Hola!";
            var output = service.Execute(new OrganizationRequest("csp_Text_Base64Encode")
            {
                Parameters = new ParameterCollection
                {
                    { "Input", input }
                }
            });
            var result = (string)output["Output"];
            Assert.Equal("wqFIb2xhIQ==", result);
        }

        [Fact]
        public void Base64Encode_EmptyString_Returns_EmptyString()
        {
            var service = Context.GetService();
            var input = string.Empty;
            var output = service.Execute(new OrganizationRequest("csp_Text_Base64Encode")
            {
                Parameters = new ParameterCollection
                {
                    { "Input", input }
                }
            });
            var result = (string)output["Output"];
            Assert.Equal(string.Empty, result);
        }
    }
}