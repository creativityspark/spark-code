using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;

namespace SparkCode
{
    public class Context
    {

        public ITracingService Tracing { get; }
        public IOrganizationService Service { get; }
        public IPluginExecutionContext PluginContext { get; }
        public CodeActivityContext WorkflowContext { get; }
        public IWorkflowContext Action { get; }


        public Context()
        {
            // provided for unit testing purposes
        }

        public Context(CodeActivityContext context)
        {
            this.WorkflowContext = context;
            this.Tracing = context.GetExtension<ITracingService>();

            if (Tracing == null)
            {
                throw new InvalidPluginExecutionException("Failed to retrieve tracing service.");
            }

            Trace($"ExecuteAction.Execute(), Activity Instance Id: {this.WorkflowContext.ActivityInstanceId}, Workflow Instance Id: {this.WorkflowContext.WorkflowInstanceId}");

            this.Action = context.GetExtension<IWorkflowContext>();
            var serviceFactory = context.GetExtension<IOrganizationServiceFactory>();
            this.Service = serviceFactory.CreateOrganizationService(null);
            Trace("Created CRM Service from context");
        }

        public Context(IServiceProvider serviceProvider)
        {
            this.PluginContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            this.Tracing =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            if (Tracing == null)
			{
				throw new InvalidPluginExecutionException("Failed to retrieve tracing service.");
			}

            Trace($"IPlugin.Execute(), MessageName: {PluginContext.MessageName}, Stage : {PluginContext.Stage}");

            IOrganizationServiceFactory serviceFactory =
                (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            this.Service = serviceFactory.CreateOrganizationService(this.PluginContext.UserId);

            Trace("Created CRM Service from context");
        }

        public T GetInputParameter<T>(string parameterName, bool required) { 
            if(required && !PluginContext.InputParameters.Contains(parameterName))
            {
                throw new ArgumentNullException($"{parameterName} is required");
            }
            var value = (T)PluginContext.InputParameters[parameterName];
            Trace($"{parameterName}:{value}");
            return value;
        }

        public T GetInputParameter<T>(InArgument parameter, string parameterName, bool required)
        {
            //TODO: Dynamically get parameter name from attributes
            T value = parameter.Get<T>(this.WorkflowContext);
            if (required && value == null)
            {
                throw new ArgumentNullException($"{parameterName} is required");
            }
            Trace($"{parameterName}:{value}");
            return value;
        }

        public void SetOutputParameter<T>(string parameterName, T value)
        {
            Trace($"{parameterName}:{value}");
            PluginContext.OutputParameters[parameterName] = value;
        }

        public void SetOutputParameter<T>(OutArgument parameter, string parameterName, T value)
        {
            Trace($"{parameterName}:{value}");
            parameter.Set(this.WorkflowContext, value);
        }

        public void Trace(string message)
        {
			Tracing?.Trace(message);
        }

        internal void Trace(string message, Exception ex)
        {
            Trace(message);
            Trace(ex.ToString());
        }
    }
}