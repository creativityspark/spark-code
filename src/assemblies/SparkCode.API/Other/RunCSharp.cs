using Microsoft.CSharp;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SparkCode.API.Other
{
    /// <displayName>Run CSharp</displayName>
    /// <summary>Compiles and executes C# 4.0 code at runtime, passing optional input JSON and custom assembly/usings.</summary>
    /// <param name="Code" type="string">C# code to execute inside the generated Main method body.</param>
    /// <param name="InputParameters" type="string" optional="true">Optional serialized JSON string passed to the runtime code.</param>
    /// <param name="ReferencedAssemblies" type="string" optional="true">Optional comma-separated assembly list added as compiler references.</param>
    /// <param name="UsingStatements" type="string" optional="true">Optional raw C# using statements appended before compilation.</param>
    /// <param name="Outputs" type="string" direction="output">Execution result returned by the runtime Main method.</param>
    /// <example>
    /// To return a value from runtime code, set Code to "outputParameters = \"hello\";".
    /// Optionally provide InputParameters as "{\"name\":\"Cris\"}" and read it from inputParameters in your runtime code.
    /// The Outputs output parameter will return the resulting string value.
    /// </example>
    public class RunCSharp : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var ctx = new Context(serviceProvider);

            // API Inputs
            string code = ctx.GetInputParameter<string>("Code", true);
            string inputParameters = ctx.PluginContext.InputParameters.Contains("InputParameters")
                ? ctx.PluginContext.InputParameters["InputParameters"] as string
                : null;
            string referencedAssemblies = ctx.PluginContext.InputParameters.Contains("ReferencedAssemblies")
                ? ctx.PluginContext.InputParameters["ReferencedAssemblies"] as string
                : null;
            string usingStatements = ctx.PluginContext.InputParameters.Contains("UsingStatements")
                ? ctx.PluginContext.InputParameters["UsingStatements"] as string
                : null;


            string[] codeToBuild = {
                "using System; using System.Dynamic; using Newtonsoft.Json; using Newtonsoft.Json.Linq;" +
                (usingStatements ?? string.Empty) +
                "namespace SparkCode {" +
                "   public class CSharpRunner {" +
                "       static public string Run(string input) {" +
                "           string output = null;" +
                code +
                "           return output;" +   
                "       }" +
                "   }" +
                "}"
            };

            // Keep legacy defaults and allow callers to append references.
            var listReferencedAssemblies = new List<string> { "System.dll", "System.Core.dll", "Microsoft.CSharp.dll", "Newtonsoft.Json.dll" };
            if (!string.IsNullOrWhiteSpace(referencedAssemblies))
            {
                listReferencedAssemblies.AddRange(referencedAssemblies
                    .Split(',')
                    .Select(assembly => assembly.Trim())
                    .Where(assembly => !string.IsNullOrWhiteSpace(assembly)));
            }

            var compilerParameters = new CompilerParameters
            {
                GenerateInMemory = true,
                TreatWarningsAsErrors = false,
                GenerateExecutable = false,
                CompilerOptions = "/optimize"
            };
            compilerParameters.ReferencedAssemblies.AddRange(listReferencedAssemblies.ToArray());

            var cSharpCodeProvider = new CSharpCodeProvider();
            CompilerResults compilerResults = cSharpCodeProvider.CompileAssemblyFromSource(compilerParameters, codeToBuild);

            if (compilerResults.Errors.HasErrors)
            {
                string exception = "Compiler Errors:";
                foreach (CompilerError compilerError in compilerResults.Errors)
                {
                    exception += Environment.NewLine + compilerError;
                }

                throw new InvalidPluginExecutionException(exception);
            }

            // Run compiled code and return the runtime output.
            Module module = compilerResults.CompiledAssembly.GetModules()[0];
            Type moduleType = module.GetType("SparkCode.CSharpRunner");
            MethodInfo methodInfo = moduleType.GetMethod("Run");
            object objResult;
            if (inputParameters == null)
            {
                objResult = methodInfo.Invoke(null,new object[] { null });
            }
            else
            {
                objResult = methodInfo.Invoke(null, new object[] { inputParameters });
            }

            // API Outputs
            string outputs = Convert.ToString(objResult);
            ctx.SetOutputParameter("Outputs", outputs);
        }
    }
}
