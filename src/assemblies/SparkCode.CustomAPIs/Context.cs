using Microsoft.Xrm.Sdk;
using System;

namespace SparkCode.CustomAPIs
{
    public class Context
    {
        private IPluginExecutionContext context;

        public ITracingService Tracing { get; }
        public IOrganizationService Service { get; }

        public Context()
        {

        }

        public Context(IServiceProvider serviceProvider)
        {
            this.context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            this.Tracing =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            if (Tracing == null)
			{
				throw new InvalidPluginExecutionException("Failed to retrieve tracing service.");
			}

			Trace($"Entered IPlugin.Execute(), MessageName: {context.MessageName}, Stage : {context.Stage}");

            IOrganizationServiceFactory serviceFactory =
                (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            this.Service = serviceFactory.CreateOrganizationService(this.context.UserId);


            Trace("Create CRM Service from context --- OK");

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