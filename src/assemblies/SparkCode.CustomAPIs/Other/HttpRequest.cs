using Microsoft.Xrm.Sdk;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SparkCode.CustomAPIs.Other
{
    /// <summary>
    /// Performs an HTTP Request with the specified Url, Method, RequestBody and Parameters and returns the Response.
    /// </summary>
    public class HttpRequest : IPlugin
    {
        Context ctx = new Context();

        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            ctx = new Context(serviceProvider);

            string url = context.InputParameters["Url"] as string ?? throw new ArgumentNullException($"Url is required");
            string method = context.InputParameters["Method"] as string ?? throw new ArgumentNullException($"Method is required");
            int timeoutSeconds = context.InputParameters.Contains("TimeoutSeconds") ? (int)context.InputParameters["TimeoutSeconds"] : 20;
            string requestBody = context.InputParameters["RequestBody"] as string;
            string param1 = context.InputParameters.Contains("Param1") ? context.InputParameters["Param1"] as string : null;
            string param2 = context.InputParameters.Contains("Param2") ? context.InputParameters["Param2"] as string : null;
            string param3 = context.InputParameters.Contains("Param3") ? context.InputParameters["Param3"] as string : null;
            string param4 = context.InputParameters.Contains("Param4") ? context.InputParameters["Param4"] as string : null;
            string param5 = context.InputParameters.Contains("Param5") ? context.InputParameters["Param5"] as string : null;

            ctx.Trace($"url:{url}");
            ctx.Trace($"requestBody:{requestBody}");
            ctx.Trace($"method:{method}");
            ctx.Trace($"param1:{param1}");
            ctx.Trace($"param2:{param2}");
            ctx.Trace($"param3:{param3}");
            ctx.Trace($"param4:{param4}");
            ctx.Trace($"param5:{param5}");
            ctx.Trace($"timeoutSeconds:{timeoutSeconds}");

            if(timeoutSeconds < 0 || timeoutSeconds > 60) throw new ArgumentException($"{nameof(timeoutSeconds)} out of range. Value must be > 0 < 60");

            var response = Run(url, requestBody, method, param1, param2, param3, param4, param5, timeoutSeconds);

            ctx.Trace($"response:{response}");

            context.OutputParameters["Response"] = response;

            ctx.Trace("Completed!");
        }

        public string Run(string url, string requestBody, string method, string param1, string param2, string param3, string param4, string param5, int timeoutSeconds)
        {
            ctx.Trace("Processing parameters...");

            // replace parameters if there are any
            url = ProcessParams(url, param1, param2, param3, param4, param5);
            requestBody = requestBody!=null ? ProcessParams(requestBody, param1, param2, param3, param4, param5) :  null;

            ctx.Trace($"url:{url}");
            ctx.Trace($"requestBody:{requestBody}");

            var response = ExecuteRequest(url, requestBody, method, timeoutSeconds);

            return response;
        }

        private string ProcessParams(string value, string param1, string param2, string param3, string param4, string param5)
        {
            return value.Replace("{param1}", param1).Replace("{param2}", param2).Replace("{param3}",param3).Replace("{param4}",param4).Replace("{param5}",param5) ;
        }

        private string responseBody = null;
        internal string ExecuteRequest(string url, string requestBody, string method, int timeoutSeconds)
        {
            ctx.Trace("Executing Request...");

            var httpClient = new HttpClient();
            httpClient.Timeout = new TimeSpan(0, 0, timeoutSeconds); 
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var request = new HttpRequestMessage();
            request.Method = new HttpMethod(method);
            request.RequestUri = new Uri(url);

            if(requestBody!=null)  request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            SendRequest(httpClient, request).Wait();

            return responseBody;
        }

        private async Task SendRequest(HttpClient client, HttpRequestMessage request)
        {
            ctx.Trace("Sending Request...");

            var response = await client.SendAsync(request);
            responseBody = await response.Content.ReadAsStringAsync();

            ctx.Trace("Request completed");
        }
    }
}
