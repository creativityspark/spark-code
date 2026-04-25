using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;

namespace SparkCode.API.Dataverse
{
    /// <displayName>Calculate Rollup Field</displayName>
    /// <summary>Triggers Dataverse rollup field recalculation for a target record.</summary>
    /// <param name="FieldName" type="string">Logical name of the rollup field to calculate.</param>
    /// <param name="TargetId" type="string">Record ID of the target row as a GUID string.</param>
    /// <param name="TargetLogicalName" type="string">Logical name of the target table.</param>
    public class CalculateRollupField : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var ctx = new Context(serviceProvider);

            // API Inputs
            string fieldName = ctx.GetInputParameter<string>("FieldName", true);
            string targetId = ctx.GetInputParameter<string>("TargetId", true);
            string targetLogicalName = ctx.GetInputParameter<string>("TargetLogicalName", true);

            ctx.Trace($"FieldName: {fieldName}");
            ctx.Trace($"TargetId: {targetId}");
            ctx.Trace($"TargetLogicalName: {targetLogicalName}");

            // Run Logic
            var calculateRequest = new CalculateRollupFieldRequest
            {
                FieldName = fieldName,
                Target = new EntityReference(targetLogicalName, Guid.Parse(targetId))
            };

            ctx.Service.Execute(calculateRequest);
        }
    }
}