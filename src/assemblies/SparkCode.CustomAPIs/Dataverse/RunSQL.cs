using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Xrm.Sdk.Organization;
using Microsoft.Identity.Client;

namespace SparkCode.CustomAPIs.Dataverse
{
    // https://joegill.com/custom-api-dataverse-sql/
    // https://learn.microsoft.com/en-us/power-platform/admin/set-up-managed-identity
    // https://itmustbecode.com/how-to-secure-a-dataverse-plug-in-with-managed-identity-using-plugin-identity-manager-for-xrmtoolbox/
    public class RunSQL : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            tracingService.Trace("Custom API - Dataverse SQL Plugin Started");

            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            var orgRequest = new RetrieveCurrentOrganizationRequest();
            var orgResponse = (RetrieveCurrentOrganizationResponse)service.Execute(orgRequest);
            string environmentUrl = orgResponse.Detail.Endpoints.Where(e => e.Key == EndpointType.WebApplication).FirstOrDefault().Value;
            tracingService.Trace("Dataverse Environment URL: " + environmentUrl);
            string sqlQuery = context.InputParameters["sqlquery"].ToString();
            string tok = GetToken();
            context.OutputParameters["sqlresults"] = QuerySqlServer(environmentUrl, tok, sqlQuery, tracingService);

        }
        static string GetToken()
        {
            string clientSecret = Environment.GetEnvironmentVariable("Secret");
            string tenantId = Environment.GetEnvironmentVariable("TenantId");
            string clientId = Environment.GetEnvironmentVariable("ClientId");
            string sqlDatabaseUrl = Environment.GetEnvironmentVariable("Url");

            var confidentialClient = ConfidentialClientApplicationBuilder.Create(clientId)
                .WithClientSecret(clientSecret)
                .WithAuthority(new Uri($"https://login.microsoftonline.com/{tenantId}"))
                .Build();

            string[] scopes = new[] { $"{sqlDatabaseUrl}/.default" };
            var authResult = confidentialClient.AcquireTokenForClient(scopes);
            return authResult.ExecuteAsync().Result.AccessToken;
        }

        private string QuerySqlServer(string environmentUrl, string accessToken, string query, ITracingService tracingService)
        {
            tracingService.Trace("QuerySqlServer: " + query);
            string database = environmentUrl.Replace("https://", "").TrimEnd('/');
            string result;
            string connectionString = $"Server={database}; Encrypt=True";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.AccessToken = accessToken;
                connection.Open();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(reader);
                        result = ConvertDataTableToJson(dataTable);
                        tracingService.Trace(result);
                    }
                }
            }
            return result;
        }
        private string ConvertDataTableToJson(DataTable dataTable)
        {
            var list = new List<Dictionary<string, object>>();
            foreach (DataRow row in dataTable.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (DataColumn column in dataTable.Columns)
                {
                    dict[column.ColumnName] = row[column];
                }
                list.Add(dict);
            }

            return JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });

        }
    }
}
