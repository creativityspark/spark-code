using System;
using System.Dynamic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SparkCode
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Running tester...");
            string input = "Cris";
            string output = CSharpRunner.Run(input);
            Console.WriteLine(output);
            Console.WriteLine("Done.");
        }
    }

    internal static class CSharpRunner
    {
        static public string Run(string input)
        {
            string output = null;
            // add your code here
            output = "Hello " + input;
            return output;
        }
    }
}
