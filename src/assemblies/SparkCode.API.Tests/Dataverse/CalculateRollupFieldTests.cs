using Microsoft.Xrm.Sdk;
using System;
using Xunit;

namespace SparkCode.API.Tests.Dataverse
{
    public class CalculateRollupFieldTests
    {
        [Fact]
        public void CalculateRollupField_InvalidTargetId_Throws_FormatException()
        {
            var service = new Context().Service;

            Assert.ThrowsAny<Exception>(() =>
            {
                service.Execute(new OrganizationRequest("csp_Dataverse_CalculateRollupField")
                {
                    Parameters = new ParameterCollection
                    {
                        { "FieldName", "new_nonexistentrollupfield" },
                        { "TargetId", "not-a-guid" },
                        { "TargetLogicalName", "account" }
                    }
                });
            });
        }

        [Fact]
        public void CalculateRollupField_InvalidTarget_Throws_Exception()
        {
            var service = new Context().Service;

            Assert.ThrowsAny<Exception>(() =>
            {
                service.Execute(new OrganizationRequest("csp_Dataverse_CalculateRollupField")
                {
                    Parameters = new ParameterCollection
                    {
                        { "FieldName", "new_nonexistentrollupfield" },
                        { "TargetId", Guid.NewGuid().ToString() },
                        { "TargetLogicalName", "account" }
                    }
                });
            });
        }
    }
}