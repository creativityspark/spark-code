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

            var result = ServiceExtensions.GetSavedQuery(service, viewId, null, null);

            Assert.NotNull(result);
            Assert.Equal(viewId, result.Id);
        }

        [Fact]
        public void GetSavedQuery_WithInvalidViewName_ThrowsException()
        {
            var service = new Context().Service;

            Assert.Throws<Exception>(() => ServiceExtensions.GetSavedQuery(service, null, 1, "ThisViewDoesNotExist_XYZ123"));
        }

        [Fact]
        public void GetViewData_ByNameAndTable_ReturnsEntityCollection()
        {
            var service = new Context().Service;

            var result = ServiceExtensions.GetViewData(service, null, "account", "Active Accounts");

            Assert.NotNull(result);
        }

        [Fact]
        public void GetViewData_ByViewId_ReturnsEntityCollection()
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

            var result = ServiceExtensions.GetViewData(service, viewId, null, null);

            Assert.NotNull(result);
        }

        [Fact]
        public void GetViewData_WithInvalidViewName_ThrowsException()
        {
            var service = new Context().Service;

            Assert.Throws<Exception>(() => ServiceExtensions.GetViewData(service, null, "account", "ThisViewDoesNotExist_XYZ123"));
        }

        [Fact]
        public void GetQueryFriendlyNames_WithValidFetchXml_ReturnsFriendlyNameEntity()
        {
            var service = new Context().Service;
            var fetchXml = "<fetch><entity name='account'><attribute name='name'/><attribute name='createdon'/></entity></fetch>";

            var result = ServiceExtensions.GetFriendlyNames(service, fetchXml);

            Assert.NotNull(result);
            Assert.True(result.Attributes.Contains("name"));
            Assert.True(result.Attributes.Contains("createdon"));
            Assert.Equal("Account Name",result.GetAttributeValue<string>("name"));
            Assert.Equal("Created On",result.GetAttributeValue<string>("createdon"));
        }

        [Fact]
        public void GetQueryFriendlyNames_WithUnknownAttribute_FallsBackToLogicalName()
        {
            var service = new Context().Service;
            var fetchXml = "<fetch><entity name='account'><attribute name='new_notrealattribute'/></entity></fetch>";

            var result = ServiceExtensions.GetFriendlyNames(service, fetchXml);

            Assert.NotNull(result);
            Assert.True(result.Attributes.Contains("new_notrealattribute"));
            Assert.Equal("new_notrealattribute", result.GetAttributeValue<string>("new_notrealattribute"));
        }

        [Fact]
        public void GetQueryFriendlyNames_WithInvalidTableName_ThrowsException()
        {
            var service = new Context().Service;
            var fetchXml = "<fetch><entity name='notanentity_invalid'><attribute name='name'/></entity></fetch>";

            Assert.ThrowsAny<Exception>(() => ServiceExtensions.GetFriendlyNames(service, fetchXml));
        }
    }
}
