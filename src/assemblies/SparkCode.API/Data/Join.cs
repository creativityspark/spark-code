using Microsoft.Xrm.Sdk;
using System;

namespace SparkCode.API.Data
{
    /// <displayName>Join</displayName>
    /// <summary>Joins two JSON arrays by matching key fields and returns a merged result collection.</summary>
    /// <param name="List1" type="string">Json string containing the first array of objects to join.</param>
    /// <param name="List2" type="string">Json string containing the second array of objects to join.</param>
    /// <param name="Field1" type="string">Field name on List1 used as the join key.</param>
    /// <param name="Field2" type="string">Field name on List2 used as the join key.</param>
    /// <param name="Results" type="entitycollection" direction="output">Joined rows with List2 fields prefixed using Field1.</param>
    /// <example>
    /// Inputs:
    /// List1: [{"id":1,"name":"order1","customerid":1},{"id":2,"name":"order2","customerid":2}]
    /// List2: [{"id":1,"name":"account1"},{"id":2,"name":"account2"}]
    /// Field1: customerid
    /// Field2: id
    ///
    /// Results (entitycollection) contains rows with Dataverse-safe linked fields:
    /// customerid_id, customerid_name
    ///
    /// ResultsJson sample:
    /// [{"id":1,"name":"order1","customerid":1,"customerid.id":1,"customerid.name":"account1"},{"id":2,"name":"order2","customerid":2,"customerid.id":2,"customerid.name":"account2"}]
    /// </example>
    public class Join : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var ctx = new Context(serviceProvider);

            // API Inputs
            var list1 = ctx.GetInputParameter<string>("List1", true);
            var list2 = ctx.GetInputParameter<string>("List2", true);
            var field1 = ctx.GetInputParameter<string>("Field1", true);
            var field2 = ctx.GetInputParameter<string>("Field2", true);

            // Run Logic
            var joinResult = SparkCode.Data.Join.JoinCollectionsWithJson(list1, list2, field1, field2);

            // API Outputs
            ctx.SetOutputParameter("Results", joinResult.Results);
            ctx.SetOutputParameter("ResultsJson", joinResult.ResultsJson);
        }
    }
}
