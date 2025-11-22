using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;

namespace SparkCode.CustomAPIs.Dataverse
{
    /// <summary>
    /// Updates a rollup field on a specified record by triggering the calculation of the rollup field.
    /// </summary>
    public class CalculateRollupField : IPlugin
    {
        Context ctx = new Context();

        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            ctx = new Context(serviceProvider);

            // Extract input parameters
            string fieldName = context.InputParameters["fieldName"] as string ?? throw new ArgumentNullException("fieldName");
            string targetId = context.InputParameters["targetId"] as string ?? throw new ArgumentNullException("targetId");
            string targetLogicalName = context.InputParameters["targetLogicalName"] as string ?? throw new ArgumentNullException("targetLogicalName");

            ctx.Trace($"FieldName: {fieldName}");
            ctx.Trace($"TargetRecordId: {targetId}");
            ctx.Trace($"TargetRecordType: {targetLogicalName}");

            // Create and execute the rollup field calculation request
            var calculateRequest = new CalculateRollupFieldRequest
            {
                FieldName = fieldName,
                Target = new EntityReference
                {
                    LogicalName = targetLogicalName,
                    Id = Guid.Parse(targetId)
                }
            };

            ctx.Service.Execute(calculateRequest);
            ctx.Trace("CalculateRollupFieldRequest executed successfully.");
        }
    }
}