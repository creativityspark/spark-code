using Microsoft.Xrm.Sdk;
using System.Text.Json;
using Xunit;

namespace SparkCode.API.Tests.PowerApps
{
    public class GetOrganizationDetailsTests
    {
        [Fact]
        public void GetOrganizationDetails_Returns_Details()
        {
            var service = new Context().Service;

            var output = service.Execute(new OrganizationRequest("csp_PowerApps_GetOrganizationDetails"));

            var details = (Entity)output["Output"];
            var endpoints = (Entity)details["Endpoints"];
            Assert.False(string.IsNullOrWhiteSpace((string)details["FriendlyName"]));
            Assert.False(string.IsNullOrWhiteSpace((string)details["UniqueName"]));
            Assert.False(string.IsNullOrWhiteSpace((string)details["OrganizationId"]));
            Assert.True(endpoints.Contains("WebApplication"));
        }

        [Fact]
        public void GetOrganizationDetailsJson_Returns_OutputJson()
        {
            var service = new Context().Service;

            var output = service.Execute(new OrganizationRequest("csp_PowerApps_GetOrganizationDetailsJson"));

            var json = JsonDocument.Parse((string)output["OutputJson"]);
            Assert.False(string.IsNullOrWhiteSpace(json.RootElement.GetProperty("FriendlyName").GetString()));
            Assert.False(string.IsNullOrWhiteSpace(json.RootElement.GetProperty("Endpoints").GetProperty("WebApplication").GetString()));
        }
    }
}
