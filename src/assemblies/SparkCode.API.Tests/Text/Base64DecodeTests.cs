using Microsoft.Xrm.Sdk;
using System;
using Xunit;

namespace SparkCode.API.Tests.Text
{
    public class Base64DecodeTests
    {
        [Fact]
        public void Base64Decode_ValidBase64_Returns_Utf8_String()
        {
            var service = Context.GetService();
            var base64 = "SGVsbG8gV29ybGQ=";
            var output = service.Execute(new OrganizationRequest("csp_Text_Base64Decode")
            {
                Parameters = new ParameterCollection
                {
                    { "Input", base64 }
                }
            });
            var result = (string)output["Output"];
            Assert.Equal("Hello World", result);
        }

        [Fact]
        public void Base64Decode_ValidBase64WithUnicode_Returns_Utf8_String()
        {
            var service = Context.GetService();
            var base64 = "wqFIb2xhIQ==";
            var output = service.Execute(new OrganizationRequest("csp_Text_Base64Decode")
            {
                Parameters = new ParameterCollection
                {
                    { "Input", base64 }
                }
            });
            var result = (string)output["Output"];
            Assert.Equal("\u00A1Hola!", result);
        }

        [Fact]
        public void Base64Decode_InvalidBase64_Throws_Exception()
        {
            var service = Context.GetService();
            var invalidBase64 = "not_base64";
            Assert.ThrowsAny<Exception>(() =>
            {
                service.Execute(new OrganizationRequest("csp_Text_Base64Decode")
                {
                    Parameters = new ParameterCollection
                    {
                        { "Input", invalidBase64 }
                    }
                });
            });
        }
    }
}