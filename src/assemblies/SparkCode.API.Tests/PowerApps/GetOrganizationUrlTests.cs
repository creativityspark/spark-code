using Microsoft.Xrm.Sdk;
using System;
using Xunit;

namespace SparkCode.API.Tests.PowerApps
{
    public class GetOrganizationUrlTests
    {
        private static string Run(IOrganizationService service, string type)
        {
            var request = new OrganizationRequest("csp_PowerApps_GetOrganizationUrl");
            if (type != null)
            {
                request.Parameters.Add("Type", type);
            }
            return (string)service.Execute(request)["Output"];
        }

        [Fact]
        public void GetOrganizationUrl_NoType_Returns_WebApplicationUrl()
        {
            var service = new Context().Service;
            var url = Run(service, null);
            Assert.False(string.IsNullOrWhiteSpace(url));
            Assert.Equal(Run(service, "WebApplication"), url);
        }

        [Theory]
        [InlineData("WebApplication")]
        [InlineData("OrganizationService")]
        [InlineData("OrganizationDataService")]
        public void GetOrganizationUrl_ValidType_Returns_Url(string type)
        {
            var service = new Context().Service;
            var url = Run(service, type);
            Assert.True(Uri.IsWellFormedUriString(url, UriKind.Absolute));
        }

        [Fact]
        public void GetOrganizationUrl_InvalidType_Throws_Exception()
        {
            var service = new Context().Service;
            Assert.ThrowsAny<Exception>(() => Run(service, "NotAnEndpoint"));
        }
    }
}
