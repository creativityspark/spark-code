using Microsoft.Xrm.Sdk;
using System;

namespace SparkCode.API.Dataverse
{
    /// <displayName>Get View Data</displayName>
    /// <summary>Retrieves records from a Dataverse saved view by ViewId or by TableName and ViewName.</summary>
    /// <param name="ViewId" type="guid" optional="true">Saved query identifier. Optional when TableName and ViewName are provided.</param>
    /// <param name="TableName" type="string" optional="true">Logical name of the table that owns the view. Required when ViewId is not provided.</param>
    /// <param name="ViewName" type="string" optional="true">Saved query name. Required when ViewId is not provided.</param>
    /// <param name="Results" type="entitycollection" direction="output">Records returned by the view FetchXML.</param>
    public class GetViewData : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var ctx = new Context(serviceProvider);

            Guid? viewId = null;
            if (ctx.PluginContext.InputParameters.Contains("ViewId"))
            {
                var inputViewId = (Guid)ctx.PluginContext.InputParameters["ViewId"];
                if (inputViewId != Guid.Empty)
                {
                    viewId = inputViewId;
                }
            }

            var tableName = ctx.PluginContext.InputParameters.Contains("TableName")
                ? ctx.PluginContext.InputParameters["TableName"] as string
                : null;
            var viewName = ctx.PluginContext.InputParameters.Contains("ViewName")
                ? ctx.PluginContext.InputParameters["ViewName"] as string
                : null;

            if (!viewId.HasValue && (string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(viewName)))
            {
                throw new InvalidPluginExecutionException("You must provide either ViewId or both TableName and ViewName.");
            }

            var results = ctx.Service.GetViewData(viewId, tableName, viewName);

            ctx.SetOutputParameter("Results", results);
            ctx.SetOutputParameter("ResultsJson", results.ToJson());
        }
    }
}
