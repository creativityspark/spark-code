using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace SparkCode.API.Dataverse
{
    /// <summary>
    /// A plugin that executes a FetchXML query and returns the results.
    /// </summary>
    /// <displayName>Run FetchXML</displayName>
    /// <param name="FetchXml" type="string">The FetchXML query to execute.</param>
    /// <param name="Results" type="entitycollection" direction="output">The results of the FetchXML query.</param>
    /// <example>
    /// To retrieve the primary contact for the current account, pass the FetchXml parameter as
    /// <fetch top='1'>
    ///     <entity name='account'>
    ///         <attribute name='name' />
    ///         <attribute name='primarycontactid' />
    ///         <filter>
    ///             <condition attribute='name' operator='eq' value='Contoso' />
    ///         </filter>
    ///     </entity>
    /// </fetch>
    /// The Results output parameter will contain the matching account record and selected fields.
    /// </example>
    public class RunFetchXml : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var ctx = new Context(serviceProvider);

            // API Inputs
            string fetchXml = ctx.GetInputParameter<string>("FetchXml", true);

            // Run Logic
            var results = ExecuteFetchXml(ctx, fetchXml);

            // API Outputs
            ctx.SetOutputParameter("Results", results);
            ctx.SetOutputParameter("ResultsJson", results.ToJson());
        }

        private EntityCollection ExecuteFetchXml(Context ctx, string fetchXml)
        {
            // Execute FetchXml
            var fetchExpression = new FetchExpression(fetchXml);
            var results = ctx.Service.RetrieveMultiple(fetchExpression);

            return results;
        }
    }
}
