using Microsoft.Xrm.Sdk;
using System;
using Xunit;

namespace SparkCode.API.Tests.Other
{
    public class HttpRequestTests
    {
        [Fact]
        public void HttpRequest_ValidGet_Returns_Response()
        {
            var service = new Context().Service;

            var output = service.Execute(new OrganizationRequest("csp_Other_HttpRequest")
            {
                Parameters = new ParameterCollection
                {
                    { "Url", "https://postman-echo.com/get?source=sparkcode" },
                    { "Method", "GET" }
                }
            });

            Assert.True(output.Results.Contains("Response"), "Expected output parameter 'Response' was not returned.");
            var response = output["Response"] as string;
            Assert.NotNull(response);
            Assert.Contains("\"source\":\"sparkcode\"", response);
        }

        [Fact]
        public void HttpRequest_WithDoubleCurlyParams_Replaces_Url_And_RequestBody()
        {
            var service = new Context().Service;

            var output = service.Execute(new OrganizationRequest("csp_Other_HttpRequest")
            {
                Parameters = new ParameterCollection
                {
                    { "Url", "https://postman-echo.com/post?name={{param1}}" },
                    { "Method", "POST" },
                    { "RequestBody", "{\"hello\":\"{{param2}}\"}" },
                    { "Param1", "Cris" },
                    { "Param2", "World" }
                }
            });

            var response = output["Response"] as string;
            Assert.NotNull(response);
            Assert.Contains("\"name\":\"Cris\"", response);
            Assert.Contains("\"hello\":\"World\"", response);
        }

        [Fact]
        public void HttpRequest_WithoutTimeout_Uses_Default_30()
        {
            var service = new Context().Service;

            var output = service.Execute(new OrganizationRequest("csp_Other_HttpRequest")
            {
                Parameters = new ParameterCollection
                {
                    { "Url", "https://postman-echo.com/get?timeout=default" },
                    { "Method", "GET" }
                }
            });

            Assert.True(output.Results.Contains("Response"), "Expected output parameter 'Response' was not returned.");
            Assert.NotNull(output["Response"] as string);
        }

        [Fact]
        public void HttpRequest_InvalidTimeout_Throws_Exception()
        {
            var service = new Context().Service;

            var exception = Assert.ThrowsAny<Exception>(() =>
            {
                service.Execute(new OrganizationRequest("csp_Other_HttpRequest")
                {
                    Parameters = new ParameterCollection
                    {
                        { "Url", "https://postman-echo.com/get" },
                        { "Method", "GET" },
                        { "TimeoutSeconds", 61 }
                    }
                });
            });

            Assert.Contains("timeoutSeconds out of range", exception.Message, StringComparison.OrdinalIgnoreCase);
        }
    }
}
