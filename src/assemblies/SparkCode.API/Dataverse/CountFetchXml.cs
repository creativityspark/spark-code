using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Xml.Linq;

namespace SparkCode.API.Dataverse
{
    /// <summary>
    /// A plugin that executes a FetchXML query and returns the number of records it matches.
    /// </summary>
    /// <displayName>Count FetchXML Records</displayName>
    /// <param name="FetchXml" type="string">The FetchXML query to count records for.</param>
    /// <param name="Count" type="integer" direction="output">The total number of records returned by the query.</param>
    /// <example>
    /// To count the number of active accounts, pass the FetchXml parameter as
    /// <fetch>
    ///     <entity name='account'>
    ///         <filter>
    ///             <condition attribute='statecode' operator='eq' value='0' />
    ///         </filter>
    ///     </entity>
    /// </fetch>
    /// The Count output parameter will contain the number of matching account records.
    /// </example>
    public class CountFetchXml : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var ctx = new Context(serviceProvider);

            // API Inputs
            string fetchXml = ctx.GetInputParameter<string>("FetchXml", true);

            // Run Logic
            var count = CountRecords(ctx, fetchXml);

            // API Outputs
            ctx.SetOutputParameter("Count", count);
        }

        private int CountRecords(Context ctx, string fetchXml)
        {
            var doc = XDocument.Parse(fetchXml);
            var fetchElement = doc.Root;

            // "top" cannot be combined with paging, so honor it with a single request
            if (fetchElement.Attribute("top") != null)
            {
                var singleResults = ctx.Service.RetrieveMultiple(new FetchExpression(doc.ToString()));
                return singleResults.Entities.Count;
            }

            // Page through all results to count records beyond the 5000 per-page limit
            fetchElement.SetAttributeValue("count", 5000);

            var count = 0;
            var pageNumber = 1;
            string pagingCookie = null;

            while (true)
            {
                fetchElement.SetAttributeValue("page", pageNumber);
                fetchElement.SetAttributeValue("paging-cookie", pagingCookie);

                var results = ctx.Service.RetrieveMultiple(new FetchExpression(doc.ToString()));
                count += results.Entities.Count;

                if (!results.MoreRecords)
                {
                    break;
                }

                pagingCookie = results.PagingCookie;
                pageNumber++;
            }

            return count;
        }
    }
}
