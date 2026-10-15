using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using Xunit;

namespace SparkCode.API.Tests.PowerApps
{
    public class GetCanvasAppAttributeTests
    {
        private static string GetExistingCanvasAppName(IOrganizationService service)
        {
            var query = new QueryExpression("canvasapp")
            {
                ColumnSet = new ColumnSet("name"),
                Criteria = new FilterExpression
                {
                    Conditions = { new ConditionExpression("name", ConditionOperator.NotNull) }
                },
                TopCount = 1
            };
            return service.RetrieveMultiple(query).Entities[0].GetAttributeValue<string>("name");
        }

        private static OrganizationResponse Run(IOrganizationService service, string app, string attribute)
        {
            return service.Execute(new OrganizationRequest("csp_PowerApps_GetCanvasAppAttribute")
            {
                Parameters = new ParameterCollection
                {
                    { "App", app },
                    { "Attribute", attribute }
                }
            });
        }

        [Fact]
        public void GetCanvasAppAttribute_ExistingApp_Returns_AttributeValue()
        {
            var service = new Context().Service;
            var appName = GetExistingCanvasAppName(service);

            var output = Run(service, appName, "name");

            Assert.Equal(appName, (string)output["Output"]);
        }

        [Fact]
        public void GetCanvasAppAttribute_NonStringAttribute_Returns_StringValue()
        {
            var service = new Context().Service;
            var appName = GetExistingCanvasAppName(service);

            var output = Run(service, appName, "canvasappid");

            Assert.True(Guid.TryParse((string)output["Output"], out _));
        }

        [Fact]
        public void GetCanvasAppAttribute_MissingApp_Returns_NullOutput()
        {
            var service = new Context().Service;

            var output = Run(service, "missing_canvas_app_name_12345", "name");

            Assert.Null(output["Output"]);
        }

        [Fact]
        public void GetCanvasAppAttribute_MissingAttribute_Returns_NullOutput()
        {
            var service = new Context().Service;
            var appName = GetExistingCanvasAppName(service);

            var output = Run(service, appName, "attribute_that_does_not_exist_12345");

            Assert.Null(output["Output"]);
        }
    }
}
