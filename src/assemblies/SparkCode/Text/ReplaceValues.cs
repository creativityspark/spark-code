using Microsoft.Xrm.Sdk;
using System;
using XrmEntitySerializer;

namespace SparkCode.API.Text
{
    public class ReplaceValues : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var ctx = new SparkCode.Context(serviceProvider);

            // API Inputs
            string text = ctx.GetInputParameter<string>("Text", true);
            string param1 = ctx.GetInputParameter<string>("Param1", true);
            string param2 = ctx.GetInputParameter<string>("Param2", false);
            string param3 = ctx.GetInputParameter<string>("Param3", false);

            // Run Logic
            var result = Replace(ctx, text, param1, param2, param3);

            // API Outputs
            ctx.SetOutputParameter("Results", result);
        }

        public string Replace(Context ctx, string text, string param1, string param2, string param3)
        {
            text = text.Replace("{{param1}}", param1);
            if(param2!=null) text = text.Replace("{{param2}}", param2);
            if (param3 != null) text = text.Replace("{{param3}}", param3);
            return text;
        }
    }
}
