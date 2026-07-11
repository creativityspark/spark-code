using Microsoft.Xrm.Sdk;
using System;

namespace SparkCode.API.Other
{
    /// <displayName>Run CSharp</displayName>
    /// <summary>Compiles and executes C# 4.0 code at runtime, passing optional typed input/output definitions, input JSON, and custom assembly/usings.</summary>
    /// <param name="Code" type="string">C# code to execute inside the generated Main method body.</param>
    /// <param name="InputParameters" type="string" optional="true">Optional serialized JSON string passed to the runtime code.</param>
    /// <param name="Types" type="string" optional="true">Optional C# type declarations available to the runtime code and conversion pipeline.</param>
    /// <param name="InputTypeName" type="string" optional="true">Optional runtime input type name. Defaults to string. Supports simple or namespace-qualified names.</param>
    /// <param name="OutputTypeName" type="string" optional="true">Optional runtime output type name. Defaults to string. Native types return Convert.ToString(result); custom types return JSON.</param>
    /// <param name="ReferencedAssemblies" type="string" optional="true">Optional comma-separated assembly list added as compiler references.</param>
    /// <param name="UsingStatements" type="string" optional="true">Optional raw C# using statements appended before compilation.</param>
    /// <param name="Outputs" type="string" direction="output">Execution result returned by the runtime Main method.</param>
    /// <example>
    /// Untyped mode: set Code to "output = \"hello\";" and optionally read input from variable input.
    /// Typed mode: set Types to class declarations, InputTypeName to your input class, and OutputTypeName to your output class.
    /// InputParameters "{\"MyProperty\":123,\"MyValue\":\"abc\"}" with InputTypeName="InputType" will deserialize into InputType before code execution.
    /// OutputTypeName="OutputType" returns Outputs as serialized JSON for custom types.
    ///
    /// Additional guidance: a local tester is available at src/assemblies/SparkCode.RunCSharpTester/Program.cs.
    /// Use it to prototype your script and then copy each part to the custom API parameters:
    /// Step 1 (optional): add additional using statements in the tester; copy them to UsingStatements.
    /// Step 2 (optional): define custom classes in the tester; copy them to Types.
    /// Step 3 (optional): set inputType/outputType aliases in the tester; use matching values in InputTypeName and OutputTypeName.
    /// Step 4: write your runtime logic inside CSharpRunner.Run and assign output; copy that logic to Code.
    /// InputParameters provides the runtime input value for input.
    /// </example>
    public class RunCSharp : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var ctx = new Context(serviceProvider);

            // API Inputs
            string code = ctx.GetInputParameter<string>("Code", true);
            string inputParameters = ctx.GetInputParameter<string>("InputParameters", false, null);
            string types = ctx.GetInputParameter<string>("Types", false, null);
            string inputTypeName = ctx.GetInputParameter<string>("InputTypeName", false, "string");
            string outputTypeName = ctx.GetInputParameter<string>("OutputTypeName", false, "string");
            string referencedAssemblies = ctx.GetInputParameter<string>("ReferencedAssemblies", false, null);
            string usingStatements = ctx.GetInputParameter<string>("UsingStatements", false, null);

            // Run Logic
                string outputs = SparkCode.Other.RunCSharp.Execute(
                code,
                inputParameters,
                types,
                inputTypeName,
                outputTypeName,
                referencedAssemblies,
                usingStatements);

            // API Outputs
            ctx.SetOutputParameter("Outputs", outputs);
        }
    }
}
