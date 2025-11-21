using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;

namespace SparkCode.CustomAPIs
{
    /// <summary>
    /// Updates a rollup field on a specified record by triggering the calculation of the rollup field.
    /// </summary>
    public class UpdateRollupField : IPlugin
    {
        Context ctx = new Context();

        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            ctx = new Context(serviceProvider);

            // Extract input parameters
            string ColumnName = context.InputParameters["ColumnName"] as string ?? throw new ArgumentNullException("ColumnName");
            string TargetRecordId = context.InputParameters["TargetRecordId"] as string ?? throw new ArgumentNullException("TargetRecordId");
            string TargetRecordType = context.InputParameters["TargetRecordType"] as string ?? throw new ArgumentNullException("TargetRecordType");

            ctx.Trace($"FieldName: {ColumnName}");
            ctx.Trace($"TargetRecordId: {TargetRecordId}");
            ctx.Trace($"TargetRecordType: {TargetRecordType}");

            // Create and execute the rollup field calculation request
            var calculateRequest = new CalculateRollupFieldRequest
            {
                FieldName = ColumnName,
                Target = new EntityReference
                {
                    LogicalName = TargetRecordType,
                    Id = Guid.Parse(TargetRecordId)
                }
            };

            ctx.Service.Execute(calculateRequest);
            ctx.Trace("CalculateRollupFieldRequest executed successfully.");
        }
    }
}