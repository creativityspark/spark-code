using Microsoft.Xrm.Sdk;
using SparkCode;
using System;
using System.ServiceModel;
using Xunit;
using Microsoft.Xrm.Sdk.Query;

namespace SparkCode.Tests
{
    public class ServiceExtensionsTests
    {
        [Fact]
        public void GetEntityTypeCode_WithAccountEntity_ReturnsOne()
        {
            var service = new Context().Service;

            var result = ServiceExtensions.GetEntityTypeCode(service, "account");

            Assert.Equal(1, result);
        }

        [Fact]
        public void GetEntityTypeCode_WithContactEntity_ReturnsTwo()
        {
            var service = new Context().Service;

            var result = ServiceExtensions.GetEntityTypeCode(service, "contact");

            Assert.Equal(2, result);
        }

        [Fact]
        public void GetEntityTypeCode_WithInvalidEntityLogicalName_ThrowsException()
        {
            var service = new Context().Service;

            Assert.Throws<FaultException<OrganizationServiceFault>>(() => ServiceExtensions.GetEntityTypeCode(service, "notanentity_invalid"));
        }

        [Fact]
        public void GetSavedQuery_ByNameAndEtc_ReturnsEntity()
        {
            var service = new Context().Service;
            var accountEtc = ServiceExtensions.GetEntityTypeCode(service, "account");

            var result = ServiceExtensions.GetSavedQuery(service, null, accountEtc, "Active Accounts");

            Assert.NotNull(result);
            Assert.Equal("Active Accounts", result.GetAttributeValue<string>("name"));
        }

        [Fact]
        public void GetSavedQuery_ByViewId_ReturnsEntity()
        {
            var service = new Context().Service;

            var query = new QueryExpression("savedquery")
            {
                ColumnSet = new ColumnSet("savedqueryid", "name"),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("name", ConditionOperator.Equal, "Active Accounts")
                    }
                }
            };
            var views = service.RetrieveMultiple(query);
            var viewId = views.Entities[0].Id;

            var result = ServiceExtensions.GetSavedQuery(service, viewId, 0, null);

            Assert.NotNull(result);
            Assert.Equal(viewId, result.Id);
        }

        [Fact]
        public void GetSavedQuery_WithInvalidViewName_ThrowsException()
        {
            var service = new Context().Service;

            Assert.Throws<Exception>(() => ServiceExtensions.GetSavedQuery(service, null, 1, "ThisViewDoesNotExist_XYZ123"));
        }
    }
}
