using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace SparkCode.CustomAPIs.Dataverse
{
    public class GetViewData : IPlugin
    {
        Context ctx = new Context();

        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            ctx = new Context(serviceProvider);

            IOrganizationService userService = serviceProvider.GetOrganizationService(context.UserId);

            // Retrieve input parameters
            Guid? viewId = context.InputParameters.Contains("ViewId") ? (Guid?)context.InputParameters["ViewId"] : null;
            string tableName = context.InputParameters.Contains("TableName") ? context.InputParameters["TableName"] as string : null;
            string viewName = context.InputParameters.Contains("ViewName") ? context.InputParameters["ViewName"] as string : null;
            bool friendlyNames = context.InputParameters.Contains("FriendlyNames") ? (bool)context.InputParameters["FriendlyNames"] : false;

            // Validate inputs
            bool isViewIdInvalid = !viewId.HasValue || (viewId.HasValue && viewId.Value == Guid.Empty);
            if (isViewIdInvalid && (string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(viewName)))
            {
                throw new InvalidPluginExecutionException("You must provide either ViewId or both TableName and ViewName.");
            }

            // Call DataManager to get view data
            string jsonData = userService.GetViewData(viewId, tableName, viewName, friendlyNames);

            // Set output parameter
            context.OutputParameters["Data"] = jsonData;
        }
    }
}
