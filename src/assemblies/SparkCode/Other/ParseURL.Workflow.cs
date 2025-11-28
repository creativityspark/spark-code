using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;

namespace SparkCode.Workflows.Other
{

    public class ParseURL : CodeActivity
    {

        [RequiredArgument]
        [Input("Url")]
        public InArgument<string> Url { get; set; }

        [Output("Scheme")]
        public OutArgument<string> Scheme { get; set; }


        protected override void Execute(CodeActivityContext context)
        {
            var ctx = new SparkCode.Context(context);

            // Workflow Inputs
            string url = ctx.GetInputParameter<string>(this.Url, "URL", true);

            // Run Logic
            var results = SparkCode.Other.ParseURL.Parse(ctx, url);

            // Workflow Outputs
            ctx.SetOutputParameter(this.Scheme, "Scheme", results);
        }
    }
}
