using Microsoft.CSharp;
using Microsoft.Xrm.Sdk;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;

namespace SparkCode.CustomAPIs
{
    // Based on: https://github.com/GuidoPreite/AutomateSharp/blob/main/AutomateSharp/RunCSharpCode.cs
    // Only Supports c# 4.0 syntax https://stackoverflow.com/questions/60247122/changing-the-c-sharp-version-in-visual-studio-2019
    public class RunCSharp : IPlugin
    {
        Context ctx = new Context();

        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            ctx = new Context(serviceProvider);

            // Custom API Inputs
            string inputParameters = context.InputParameters["InputParameters"] as string;
            string referencedAssemblies = context.InputParameters["ReferencedAssemblies"] as string;
            string usingStatements = context.InputParameters["UsingStatements"] as string;
            string code = context.InputParameters["Code"] as string;

            // Trace input parameters
            ctx.Trace($"InputParameters: {inputParameters}");
            ctx.Trace($"ReferencedAssemblies: {referencedAssemblies}");
            ctx.Trace($"UsingStatements: {usingStatements}");
            ctx.Trace($"Code: {code}");

            // Custom API Outputs
            bool success = false;
            string outputParameters = "";

            string[] codeToBuild = {
            "using System; using System.Dynamic;" +
            usingStatements  + // Add user Using Statements
            "namespace AutomateSharp {"  +
            "   public class AutomateSharp {" +
            "       static public string Main(string inputParameters) {" +
            "           string outputParameters = String.Empty;" +
                        code + // Add user Code
            "           return outputParameters;" +
            "       }" +
            "   }" +
            "}"};

            // trace code to build
            ctx.Trace($"Code to build: {string.Join(Environment.NewLine, codeToBuild)}");

            List<string> listReferencedAssemblies = new List<string> { "System.dll", "System.Core.dll", "Microsoft.CSharp.dll" };

            // Add user Referenced Assemblies
            if (!string.IsNullOrWhiteSpace(referencedAssemblies))
            {
                listReferencedAssemblies.AddRange(referencedAssemblies.Split(','));
            }

            CompilerParameters compilerParameters = new CompilerParameters
            {
                GenerateInMemory = true,
                TreatWarningsAsErrors = false,
                GenerateExecutable = false,
                CompilerOptions = "/optimize"
            };
            compilerParameters.ReferencedAssemblies.AddRange(listReferencedAssemblies.ToArray());

            CSharpCodeProvider cSharpCodeProvider = new CSharpCodeProvider();
            CompilerResults compilerResults = cSharpCodeProvider.CompileAssemblyFromSource(compilerParameters, codeToBuild);

            if (compilerResults.Errors.HasErrors == true)
            {
                string exception = "Compiler Errors:";
                foreach (CompilerError ce in compilerResults.Errors) { exception += Environment.NewLine + ce.ToString(); }
                throw new Exception(exception);
            }

            // Run code
            Module module = compilerResults.CompiledAssembly.GetModules()[0];
            Type moduleType = module.GetType("AutomateSharp.AutomateSharp");
            MethodInfo methodInfo = moduleType.GetMethod("Main");
            object objResult = methodInfo.Invoke(null, new object[] { inputParameters });

            // Set values
            outputParameters = Convert.ToString(objResult);
            success = true;

            // Set OutputParameters values
            context.OutputParameters["Success"] = success;
            context.OutputParameters["OutputParameters"] = outputParameters;
        }
    }
}
