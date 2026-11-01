using Microsoft.Xrm.Sdk;
using System;
using Newtonsoft.Json;

namespace SparkCode.API.Other
{
    /// <displayName>Run CSharp</displayName>
    /// <summary>Compiles and executes C# 4.0 code at runtime, passing optional typed input/output definitions, input JSON, and custom assembly/usings. The code can call Log(string message) to collect log messages returned in Logs.</summary>
    /// <param name="Code" type="string">C# code to execute inside the generated Main method body.</param>
    /// <param name="InputParameters" type="string" optional="true">Optional serialized JSON string passed to the runtime code.</param>
    /// <param name="Types" type="string" optional="true">Optional C# type declarations available to the runtime code and conversion pipeline.</param>
    /// <param name="InputTypeName" type="string" optional="true">Optional runtime input type name. Defaults to string. Supports simple or namespace-qualified names.</param>
    /// <param name="OutputTypeName" type="string" optional="true">Optional runtime output type name. Defaults to string. Native types return Convert.ToString(result); custom types return JSON.</param>
    /// <param name="ReferencedAssemblies" type="string" optional="true">Optional comma-separated assembly list added as compiler references.</param>
    /// <param name="UsingStatements" type="string" optional="true">Optional raw C# using statements appended before compilation.</param>
    /// <param name="Outputs" type="string" direction="output">Execution result returned by the runtime Main method.</param>
    /// <param name="Logs" type="string" direction="output">Messages written by the code via Log(message), one per line.</param>
    /// <param name="CompilerErrors" type="string" direction="output">JSON array of section-relative compiler diagnostics; null when compilation succeeds.</param>
    /// <param name="RuntimeErrors" type="string" direction="output">JSON runtime/conversion diagnostics; null when absent. Source lines may be null.</param>
    /// <example>
    /// Untyped mode: set Code to "output = \"hello\";" and optionally read input from variable input.
    /// Logging: call Log(message) from Code (or CSharpRunner.Log(message) from classes declared in Types), e.g.
    /// "output = \"Hello \" + input; Log(output);" returns each logged message on its own line in Logs.
    /// Script failures return diagnostics instead of throwing; Logs includes messages written before a runtime failure.
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
    /// <remarks>
    /// Script compile/runtime and input/output conversion errors return structured diagnostics with complete pre-error logs instead of throwing; 
    /// invalid type configuration and infrastructure failures still fault.
    /// The API returns `Outputs`, `Logs`, `CompilerErrors`, and `RuntimeErrors`.
    /// Diagnostic properties contain JSON arrays or null; callers must check them before treating a successful  response as successful script execution.
    /// Outputs is null on script failure. 
    /// Input-conversion errors use section `InputParameters`; output-serialization errors use section `Code` with a null line. 
    /// Compiler diagnostics use section-relative one-based line/column positions.
    /// An error reported on generated closing code can have null section/line/column; runtime lines can be null when symbols
    /// are unavailable. Never interpret generated offsets as user-source positions.
    /// </remarks>
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
            var result = SparkCode.Other.RunCSharp.Execute(
                code,
                inputParameters,
                types,
                inputTypeName,
                outputTypeName,
                referencedAssemblies,
                usingStatements);

            // API Outputs
            ctx.SetOutputParameter("Outputs", result.Outputs);
            ctx.SetOutputParameter("Logs", result.Logs);
            ctx.SetOutputParameter("CompilerErrors", result.CompilerErrors.Count == 0 ? null : JsonConvert.SerializeObject(result.CompilerErrors));
            ctx.SetOutputParameter("RuntimeErrors", result.RuntimeErrors.Count == 0 ? null : JsonConvert.SerializeObject(result.RuntimeErrors));
        }
    }
}
