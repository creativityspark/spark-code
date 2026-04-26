using Microsoft.Xrm.Sdk;
using SparkCode;
using System;
using System.ServiceModel;
using Xunit;
using Microsoft.Xrm.Sdk.Query;

namespace SparkCode.Tests
{
    public class EntityExtensionsTests
    {
        [Fact]
        public void ToJson_WithGetViewDataResults_ReturnsJson()
        {
            var service = new Context().Service;

            var results = ServiceExtensions.GetViewData(service, null, "account", "Active Accounts");
            var json = EntityExtensions.ToJson(results);
            Assert.NotNull(json);
        }        
    }
}