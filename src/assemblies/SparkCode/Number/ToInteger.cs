using Microsoft.Xrm.Sdk;
using System;
using XrmEntitySerializer;

namespace SparkCode.API.Text
{
    public class ToInteger : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var ctx = new Context(serviceProvider);

            // API Inputs
            string text = ctx.GetInputParameter<string>("Text", true);

            // API Outputs
            ctx.SetOutputParameter("Results", int.Parse(text));
        }

    }
}
