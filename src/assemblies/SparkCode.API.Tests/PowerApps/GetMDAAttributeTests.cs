using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using Xunit;

namespace SparkCode.API.Tests.PowerApps
{
    public class GetMDAAttributeTests
    {
        private static string GetExistingAppUniqueName(IOrganizationService service)
        {
            var query = new QueryExpression("appmodule")
            {
                ColumnSet = new ColumnSet("uniquename"),
                Criteria = new FilterExpression
                {
                    Conditions = { new ConditionExpression("uniquename", ConditionOperator.NotNull) }
                },
                TopCount = 1
            };
            return service.RetrieveMultiple(query).Entities[0].GetAttributeValue<string>("uniquename");
        }

        private static OrganizationResponse Run(IOrganizationService service, string app, string attribute)
        {
            return service.Execute(new OrganizationRequest("csp_PowerApps_GetMDAAttribute")
            {
                Parameters = new ParameterCollection
                {
                    { "App", app },
                    { "Attribute", attribute }
                }
            });
        }

        [Fact]
        public void GetMDAAttribute_ExistingApp_Returns_AttributeValue()
        {
            var service = new Context().Service;
            var uniqueName = GetExistingAppUniqueName(service);

            var output = Run(service, uniqueName, "uniquename");

            Assert.Equal(uniqueName, (string)output["Output"]);
        }

        [Fact]
        public void GetMDAAttribute_GuidAttribute_Returns_StringValue()
        {
            var service = new Context().Service;
            var uniqueName = GetExistingAppUniqueName(service);

            var output = Run(service, uniqueName, "appmoduleid");

            Assert.True(Guid.TryParse((string)output["Output"], out _));
        }

        [Fact]
        public void GetMDAAttribute_MissingApp_Returns_NullOutput()
        {
            var service = new Context().Service;

            var output = Run(service, "missing_app_unique_name_12345", "name");

            Assert.Null(output["Output"]);
        }

        [Fact]
        public void GetMDAAttribute_MissingAttribute_Returns_NullOutput()
        {
            var service = new Context().Service;
            var uniqueName = GetExistingAppUniqueName(service);

            var output = Run(service, uniqueName, "attribute_that_does_not_exist_12345");

            Assert.Null(output["Output"]);
        }
    }
}
