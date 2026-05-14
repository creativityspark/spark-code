using System;

namespace SparkCode.API.Tests.Dataverse
{
    using Microsoft.Xrm.Sdk;
    using Xunit;

    public class PublishWebResourceTests
    {
        [Fact]
        public void PublishWebResource_ValidName_Returns_Template()
        {
            var service = new Context().Service;
            var webResourceName = "csp_/notifications/sample1.htm";
            var output = service.Execute(new OrganizationRequest("csp_Dataverse_PublishWebResource")
            {
                Parameters = new ParameterCollection
                {
                    { "WebResourceName", webResourceName }
                }
            });
        }

        [Fact]
        public void PublishWebResource_InvalidName_ThrowsException()
        {
            var service = new Context().Service;
            Assert.ThrowsAny<Exception>(() =>
            {
                var webResourceName = "";
                var output = service.Execute(new OrganizationRequest("csp_Dataverse_PublishWebResource")
                {
                    Parameters = new ParameterCollection
                {
                    { "WebResourceName", webResourceName }
                }
                });
            });
            
        }
    }
}