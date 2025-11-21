using SparkTools.CustomAPIs;
using Xunit;

namespace SparkTools.CustomAPIs.Tests
{
    public class HttpRequestTests
    {
        [Fact]
        public void Simple_Get_Request()
        {
            var result = new HttpRequest().Run("https://gen-endpoint.com/api/greeting", null, "GET", null, null, null, null, null, 30);
            Assert.NotNull(result);
        }

        [Fact]
        public void Get_Request_With_Results()
        {
            // Expected Result
            //{ "message":"Hello from our API!","version":"1.0.0","timestamp":"2025-07-21T12:02:15.021Z"}

            var result = new HttpRequest().Run("https://gen-endpoint.com/api/greeting", null, "GET", null, null, null, null, null, 30);
            Assert.NotNull(result);
            var jsonResult = System.Text.Json.JsonDocument.Parse(result);
            Assert.True(jsonResult.RootElement.TryGetProperty("message", out var message));
            Assert.Equal("Hello from our API!", message.GetString());
        }

        [Fact]
        public void Get_Request_With_Params()
        {
            var result = new HttpRequest().Run("https://gen-endpoint.com/{param1}/greeting", null, "GET", "api", null, null, null, null, 30);
            Assert.NotNull(result);
            var jsonResult = System.Text.Json.JsonDocument.Parse(result);
            Assert.True(jsonResult.RootElement.TryGetProperty("message", out var message));
            Assert.Equal("Hello from our API!", message.GetString());
        }

        // Simple post request
        [Fact]
        public void Simple_Post_Request()
        {
            var testData = @"
            {
              ""name"": ""Bob"",
              ""preferences"": {
                ""formal"": false
              }
            }";
            var result = new HttpRequest().Run("https://gen-endpoint.com/api/greeting", testData, "POST", null, null, null, null, null, 30);
            Assert.NotNull(result);
            var jsonResult = System.Text.Json.JsonDocument.Parse(result);
            Assert.True(jsonResult.RootElement.TryGetProperty("message", out var message));
            Assert.Equal("Greetings, Bob! Your POST request was received.", message.GetString());
        }

        // post request with params
        [Fact]
        public void Post_Request_With_Params()
        {
            var testData = @"
            {
              ""{param1}"": ""{param2}"",
              ""{param3}"": {
                ""{param4}"": {param5}
              }
            }";
            var result = new HttpRequest().Run("https://gen-endpoint.com/api/greeting", testData, "POST", "name", "Bob", "preferences", "formal", "false", 30);
            Assert.NotNull(result);
            var jsonResult = System.Text.Json.JsonDocument.Parse(result);
            Assert.True(jsonResult.RootElement.TryGetProperty("message", out var message));
            Assert.Equal("Greetings, Bob! Your POST request was received.", message.GetString());
        }

    }
}
