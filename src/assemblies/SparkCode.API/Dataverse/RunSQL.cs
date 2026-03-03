using Microsoft.Xrm.Sdk;
using System;
using System.Data;

namespace SparkCode.API.Dataverse
{
    /// <displayName>RunSQL</displayName>
    /// <summary>Executes a SQL query against the Dataverse TDS endpoint and returns the results.</summary>
    /// <param name="SQLQuery" type="string">The SQL query to execute.</param>
    /// <param name="Results" type="expando" direction="output">The results of the query as a DataSet.</param>
    /// <remarks>Taken from: https://hajekj.net/2025/11/06/calling-tds-endpoint-from-plugins/</remarks>
    public class RunSQL : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var ctx = new Context(serviceProvider);

            // API Inputs
            string sqlQuery = ctx.GetInputParameter<string>("SQLQuery", true);

            // Prepare ExecutePowerBISql request
            var request = new OrganizationRequest("ExecutePowerBISql")
            {
                Parameters = new ParameterCollection
                {
                    { "QueryText", sqlQuery }
                    // Optionally add NameMappingOptions or QueryParameters here
                }
            };

            // Execute request
            var response = ctx.Service.Execute(request);
            DataSet dsResults = (DataSet)response.Results["Records"];
            var results = dsResults.ToEntity();

            // API Outputs
            ctx.SetOutputParameter("Results", results);
            ctx.SetOutputParameter("ResultsJson", results.ToJson());
        }
    }
}
