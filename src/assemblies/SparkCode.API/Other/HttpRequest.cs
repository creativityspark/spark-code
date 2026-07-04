using Microsoft.Xrm.Sdk;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace SparkCode.API.Other
{
    /// <displayName>Http Request</displayName>
    /// <summary>Performs an HTTP request with optional body and parameter replacement using {{paramN}} placeholders.</summary>
    /// <param name="Url" type="string">Target request URL. Supports {{param1}} to {{param5}} placeholders.</param>
    /// <param name="Method" type="string">HTTP method to use (GET, POST, PUT, PATCH, DELETE, etc.).</param>
    /// <param name="TimeoutSeconds" type="int" optional="true">Request timeout in seconds. Defaults to 30 when omitted. Allowed range is 1 to 60.</param>
    /// <param name="RequestBody" type="string" optional="true">Optional request body. Supports {{param1}} to {{param5}} placeholders.</param>
    /// <param name="Param1" type="string" optional="true">Optional replacement value for {{param1}}.</param>
    /// <param name="Param2" type="string" optional="true">Optional replacement value for {{param2}}.</param>
    /// <param name="Param3" type="string" optional="true">Optional replacement value for {{param3}}.</param>
    /// <param name="Param4" type="string" optional="true">Optional replacement value for {{param4}}.</param>
    /// <param name="Param5" type="string" optional="true">Optional replacement value for {{param5}}.</param>
    /// <param name="Response" type="string" direction="output">HTTP response body as a string.</param>
    public class HttpRequest : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var ctx = new Context(serviceProvider);

            // API Inputs
            string url = ctx.GetInputParameter<string>("Url", true);
            string method = ctx.GetInputParameter<string>("Method", true);

            int timeoutSeconds = 30;
            if (ctx.PluginContext.InputParameters.Contains("TimeoutSeconds"))
            {
                timeoutSeconds = (int)ctx.PluginContext.InputParameters["TimeoutSeconds"];
            }

            // Dataverse can materialize optional int parameters as 0 when omitted.
            if (timeoutSeconds <= 0)
            {
                timeoutSeconds = 30;
            }

            if (timeoutSeconds > 60)
            {
                throw new ArgumentException($"{nameof(timeoutSeconds)} out of range. Value must be <= 60");
            }

            string requestBody = ctx.PluginContext.InputParameters.Contains("RequestBody")
                ? ctx.PluginContext.InputParameters["RequestBody"] as string
                : null;

            string[] parameters = new string[5];
            for (int i = 0; i < parameters.Length; i++)
            {
                string parameterName = $"Param{i + 1}";
                parameters[i] = ctx.PluginContext.InputParameters.Contains(parameterName)
                    ? ctx.PluginContext.InputParameters[parameterName] as string
                    : null;
            }

            // Run Logic
            string processedUrl = SparkCode.Text.ReplaceParams.Replace(url, parameters);
            string processedRequestBody = requestBody == null
                ? null
                : SparkCode.Text.ReplaceParams.Replace(requestBody, parameters);

            string response = ExecuteRequest(processedUrl, processedRequestBody, method, timeoutSeconds);

            // API Outputs
            ctx.SetOutputParameter("Response", response);
        }

        internal string ExecuteRequest(string url, string requestBody, string method, int timeoutSeconds)
        {
            using (var httpClient = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                httpClient.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                request.Method = new HttpMethod(method);
                request.RequestUri = new Uri(url);

                if (requestBody != null)
                {
                    request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                }

                var response = httpClient.SendAsync(request).GetAwaiter().GetResult();
                return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }
        }
    }
}
