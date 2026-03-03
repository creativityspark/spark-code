using Microsoft.Xrm.Sdk;
using System;

namespace SparkCode.API.Data
{
    /// <displayName>Select</displayName>
    /// <summary>
    /// Performs a JSONQuery select on the specified data and returns the results
    /// </summary>
    /// <param name="Data" type="string">Json string containing the data to be selected.</param>
    /// <param name="Query" type="string">Json Query select string.</param>
    /// <param name="Results" type="string" direction="output"> Data returned by the query in JSON format.</param>
    /// <remarks>
    /// Based on: https://www.rfc-editor.org/rfc/rfc9535.html
    /// Library used: https://www.newtonsoft.com/json/help/html/QueryJsonSelectToken.htm
    /// </remarks>
    public class Select : IPlugin
    {

        public void Execute(IServiceProvider serviceProvider)
        {
            Context ctx = new Context(serviceProvider);

            // API Inputs
            string data = ctx.GetInputParameter<string>("Data", true);
            string query = ctx.GetInputParameter<string>("Query", true);

            // Run Logic
            string results = SparkCode.Data.Select.RunQuery(ctx, data, query);

            // API Outputs
            ctx.SetOutputParameter("Results", results);
        }
    }
}
