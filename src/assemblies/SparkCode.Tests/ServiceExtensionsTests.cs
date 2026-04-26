using Microsoft.Xrm.Sdk;
using SparkCode;
using System;
using System.ServiceModel;
using Xunit;

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
    }
}
