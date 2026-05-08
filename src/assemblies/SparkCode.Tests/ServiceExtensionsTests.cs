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
        public void GetTableLogicalName_WithAccountEtc_ReturnsAccount()
        {
            var service = new Context().Service;

            var result = ServiceExtensions.GetTableLogicalName(service, 1);

            Assert.Equal("account", result);
        }

        [Fact]
        public void GetTableLogicalName_WithContactEtc_ReturnsContact()
        {
            var service = new Context().Service;

            var result = ServiceExtensions.GetTableLogicalName(service, 2);

            Assert.Equal("contact", result);
        }

        [Fact]
        public void GetTableLogicalName_WithInvalidEtc_ReturnsEmptyString()
        {
            var service = new Context().Service;

            var result = ServiceExtensions.GetTableLogicalName(service, 999999);

            Assert.Equal(string.Empty, result);
        }

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
        public void GetTableColumnNames_WithAccount_ReturnsKnownColumns()
        {
            var service = new Context().Service;

            var result = ServiceExtensions.GetTableColumnNames(service, "account");

            Assert.NotNull(result);
            Assert.Contains("accountid", result);
            Assert.Contains("name", result);
            Assert.Contains("NAME", result);
        }

        [Fact]
        public void GetTableColumnNames_WithInvalidTableName_ThrowsException()
        {
            var service = new Context().Service;

            Assert.Throws<FaultException<OrganizationServiceFault>>(() => ServiceExtensions.GetTableColumnNames(service, "notanentity_invalid"));
        }

        [Fact]
        public void GetTableColumnNames_WithNullTableName_ThrowsException()
        {
            var service = new Context().Service;

            Assert.Throws<ArgumentNullException>(() => ServiceExtensions.GetTableColumnNames(service, null));
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

        [Fact]
        public void GetFriendlyNames_WithEntityObject_ReturnsFriendlyNameEntity()
        {
            var service = new Context().Service;
            var entityObject = new Entity("account")
            {
                ["name"] = "Contoso",
                ["createdon"] = DateTime.UtcNow
            };

            var result = ServiceExtensions.GetFriendlyNames(service, entityObject);

            Assert.NotNull(result);
            Assert.True(result.Attributes.Contains("name"));
            Assert.True(result.Attributes.Contains("createdon"));
            Assert.Equal("Account Name", result.GetAttributeValue<string>("name"));
            Assert.Equal("Created On", result.GetAttributeValue<string>("createdon"));
        }

        [Fact]
        public void GetFriendlyNames_WithEntityObjectUnknownAttribute_FallsBackToLogicalName()
        {
            var service = new Context().Service;
            var entityObject = new Entity("account")
            {
                ["new_notrealattribute"] = "x"
            };

            var result = ServiceExtensions.GetFriendlyNames(service, entityObject);

            Assert.NotNull(result);
            Assert.True(result.Attributes.Contains("new_notrealattribute"));
            Assert.Equal("new_notrealattribute", result.GetAttributeValue<string>("new_notrealattribute"));
        }

        [Fact]
        public void GetFriendlyNames_WithEntityObjectWithoutLogicalName_ThrowsException()
        {
            var service = new Context().Service;
            var entityObject = new Entity();
            entityObject["name"] = "Contoso";

            Assert.Throws<ArgumentNullException>(() => ServiceExtensions.GetFriendlyNames(service, entityObject));
        }

        [Fact]
        public void GetMDAAttribute_WithExistingModelDrivenApp_ReturnsTypedValue()
        {
            var service = new Context().Service;
            var modelDrivenApp = GetExistingModelDrivenApp(service);
            var appUniqueName = modelDrivenApp.GetAttributeValue<string>("uniquename");

            var result = ServiceExtensions.GetMDAAttribute<string>(service, appUniqueName, "uniquename");

            Assert.False(string.IsNullOrWhiteSpace(result));
            Assert.Equal(appUniqueName, result);
        }

        [Fact]
        public void GetCanvasAppAttribute_WithExistingCanvasApp_ReturnsTypedValue()
        {
            var service = new Context().Service;
            var canvasApp = GetExistingCanvasApp(service);
            var appName = canvasApp.GetAttributeValue<string>("name");

            var result = ServiceExtensions.GetCanvasAppAttribute<string>(service, appName, "name");

            Assert.False(string.IsNullOrWhiteSpace(result));
            Assert.Equal(appName, result);
        }

        [Fact]
        public void GetMDAAttribute_WithInvalidApp_ReturnsDefault()
        {
            var service = new Context().Service;

            var result = ServiceExtensions.GetMDAAttribute<string>(service, "missing_app_unique_name_12345", "name");

            Assert.Null(result);
        }

        [Fact]
        public void GetCanvasAppAttribute_WithInvalidApp_ReturnsDefault()
        {
            var service = new Context().Service;

            var result = ServiceExtensions.GetCanvasAppAttribute<string>(service, "missing_canvas_app_name_12345", "name");

            Assert.Null(result);
        }

        [Fact]
        public void GetMDAAttribute_WithInvalidAttribute_ReturnsDefault()
        {
            var service = new Context().Service;
            var modelDrivenApp = GetExistingModelDrivenApp(service);
            var appUniqueName = modelDrivenApp.GetAttributeValue<string>("uniquename");

            var result = ServiceExtensions.GetMDAAttribute<string>(service, appUniqueName, "attribute_that_does_not_exist_12345");

            Assert.Null(result);
        }

        [Fact]
        public void GetCanvasAppAttribute_WithInvalidAttribute_ReturnsDefault()
        {
            var service = new Context().Service;
            var canvasApp = GetExistingCanvasApp(service);
            var appName = canvasApp.GetAttributeValue<string>("name");

            var result = ServiceExtensions.GetCanvasAppAttribute<string>(service, appName, "attribute_that_does_not_exist_12345");

            Assert.Null(result);
        }

        private static Entity GetExistingModelDrivenApp(IOrganizationService service)
        {
            var query = new QueryExpression("appmodule")
            {
                ColumnSet = new ColumnSet("appmoduleid", "uniquename", "name"),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("uniquename", ConditionOperator.NotNull)
                    }
                },
                TopCount = 1
            };

            var result = service.RetrieveMultiple(query);
            Assert.True(result.Entities.Count > 0, "No model-driven app records found in appmodule.");

            return result.Entities[0];
        }

        private static Entity GetExistingCanvasApp(IOrganizationService service)
        {
            var query = new QueryExpression("canvasapp")
            {
                ColumnSet = new ColumnSet("canvasappid", "name"),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("name", ConditionOperator.NotNull)
                    }
                },
                TopCount = 1
            };

            var result = service.RetrieveMultiple(query);
            Assert.True(result.Entities.Count > 0, "No canvas app records found in canvasapp.");

            return result.Entities[0];
        }
    }
}
