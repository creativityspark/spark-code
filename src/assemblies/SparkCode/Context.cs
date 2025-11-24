using Microsoft.Xrm.Sdk;
using System;

namespace SparkCode
{
    public class Context
    {

        public ITracingService Tracing { get; }
        public IOrganizationService Service { get; }
        public IPluginExecutionContext PluginContext { get; }

        public Context()
        {
            // provided for unit testing purposes
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

            if(PluginContext != null)
            {
                Trace($"Entered IPlugin.Execute(), MessageName: {PluginContext.MessageName}, Stage : {PluginContext.Stage}");
            }

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
            return (T)PluginContext.InputParameters[parameterName];
        }

        public void SetOutputParameter<T>(string parameterName, T value)
        {
            PluginContext.OutputParameters[parameterName] = value;
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