using Microsoft.Xrm.Sdk;
using System;

namespace SparkCode.API.Text
{
    /// <displayName>Replace Params</displayName>
    /// <summary>
    /// Replaces parameter placeholders in text with provided values.
    /// </summary>
    /// <param name="Text" type="string">Text containing parameter placeholders</param>
    /// <param name="Param1" displayName="Param 1" type="string">Value for Param1</param>
    /// <param name="Param2" displayName="Param 2" type="string" optional="true">Value for Param2</param>
    /// <param name="Param3" displayName="Param 3" type="string" optional="true">Value for Param3</param>
    /// <param name="Param4" displayName="Param 4" type="string" optional="true">Value for Param4</param>
    /// <param name="Param5" displayName="Param 5" type="string" optional="true">Value for Param5</param>
    /// <param name="Param6" displayName="Param 6" type="string" optional="true">Value for Param6</param>
    /// <param name="Param7" displayName="Param 7" type="string" optional="true">Value for Param7</param>
    /// <param name="Param8" displayName="Param 8" type="string" optional="true">Value for Param8</param>
    /// <param name="Param9" displayName="Param 9" type="string" optional="true">Value for Param9</param>
    /// <param name="Results" type="string" direction="output">Text with parameters replaced</param>
    /// <example>
    /// To replace parameters in a template, provide the Text input as "Hello {{param1}}, your code is {{param2}}." and set Param1 to "User" and Param2 to "ABC123". The Results output will be "Hello User, your code is ABC123."
    /// </example>
    public class ReplaceParams : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var ctx = new SparkCode.Context(serviceProvider);

            // API Inputs
            string text = ctx.GetInputParameter<string>("Text", true);
            string[] parameters = new string[9];
            parameters[0] = ctx.GetInputParameter<string>("Param1", true);
            parameters[1] = ctx.GetInputParameter<string>("Param2", false);
            parameters[2] = ctx.GetInputParameter<string>("Param3", false);
            parameters[3] = ctx.GetInputParameter<string>("Param4", false);
            parameters[4] = ctx.GetInputParameter<string>("Param5", false);
            parameters[5] = ctx.GetInputParameter<string>("Param6", false);
            parameters[6] = ctx.GetInputParameter<string>("Param7", false);
            parameters[7] = ctx.GetInputParameter<string>("Param8", false);
            parameters[8] = ctx.GetInputParameter<string>("Param9", false);

            // Run Logic
            var result = SparkCode.Text.ReplaceParams.Replace(ctx, text, parameters);

            // API Outputs
            ctx.SetOutputParameter("Results", result);
        }
    }
}
