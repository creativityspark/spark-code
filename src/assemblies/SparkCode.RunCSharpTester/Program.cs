using System;
using System.Dynamic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
// Step 1 (optional): add your additional using statements here
// On the custom API, this goes in the UsingStatements parameter


// Step 3 (optional): Define your input and output types here
// On the custom API, this goes in the InputTypeName and OutputTypeName parameters
using inputType = System.String; // can use SparkCode.InputType 
using outputType = System.String; // can use SparkCode.OutputType

namespace SparkCode
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Running tester...");
            inputType input = "Cris";
            outputType output = CSharpRunner.Run(input);
            Console.WriteLine(output);
            Console.WriteLine("Done.");
        }
    }

    // Step 2 (optional): Define your types here
    // On the custom API, this goes in the Types parameter
    class InputType {
        int MyProperty { get; set; }
        string MyValue{ get; set; }
    };
    class OutputType
    {
        int MyProperty { get; set; }
    };


    internal static class CSharpRunner
    {
        private static readonly System.Collections.Generic.List<string> __logs = new System.Collections.Generic.List<string>();

        // Available to your code (and to your types via CSharpRunner.Log); returned in the Logs output parameter
        public static void Log(string message)
        {
            __logs.Add(message);
            Console.WriteLine("[Log] " + message);
        }

        static public outputType Run(inputType input)
        {
            outputType output = default(outputType);

            // Step 4: add your code here
            // On the custom API, this goes in the Code parameter
            output = "Hello " + input;
            Log(output);

            // End of custom code
            return output;
        }
    }
}
