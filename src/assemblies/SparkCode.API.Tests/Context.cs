using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using System;

namespace SparkCode.API.Tests
{
    public class Context
    {
        private static IOrganizationService _service;

        public static IOrganizationService GetService()
        {
            // Get connection string from environment variable
            var connectionString = Environment.GetEnvironmentVariable("DATAVERSE_CONNECTION_STRING_TESTS");
            if(_service == null)
            {
                _service = new ServiceClient(connectionString);
            }
            return _service;
        }
    }
}
