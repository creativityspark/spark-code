using Microsoft.CSharp;
using Newtonsoft.Json;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace SparkCode.Other
{
    /// <summary>
    /// Compiles and executes C# code at runtime using optional custom input and output types.
    /// </summary>
    public static class RunCSharp
    {
        private static readonly Dictionary<string, Type> NativeTypeAliases = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            { "string", typeof(string) },
            { "bool", typeof(bool) },
            { "boolean", typeof(bool) },
            { "byte", typeof(byte) },
            { "sbyte", typeof(sbyte) },
            { "short", typeof(short) },
            { "int16", typeof(short) },
            { "ushort", typeof(ushort) },
            { "uint16", typeof(ushort) },
            { "int", typeof(int) },
            { "int32", typeof(int) },
            { "uint", typeof(uint) },
            { "uint32", typeof(uint) },
            { "long", typeof(long) },
            { "int64", typeof(long) },
            { "ulong", typeof(ulong) },
            { "uint64", typeof(ulong) },
            { "float", typeof(float) },
            { "single", typeof(float) },
            { "double", typeof(double) },
            { "decimal", typeof(decimal) },
            { "datetime", typeof(DateTime) },
            { "guid", typeof(Guid) },
            { "char", typeof(char) }
        };

        /// <summary>
        /// Builds a temporary assembly for the provided code and executes the generated <c>Run</c> method.
        /// </summary>
        /// <param name="code">Code block that assigns a value to <c>output</c>.</param>
        /// <param name="inputParameters">Input value passed to the generated method.</param>
        /// <param name="types">Optional custom type definitions used by input or output types.</param>
        /// <param name="inputTypeName">Declared input type name for the generated method signature.</param>
        /// <param name="outputTypeName">Declared output type name for the generated method signature.</param>
        /// <param name="referencedAssemblies">Optional comma-separated additional assembly references.</param>
        /// <param name="usingStatements">Optional using directives appended to the generated code.</param>
        /// <returns>Output value serialized as string for API transport.</returns>
        /// <exception cref="Exception">Thrown when compilation, type resolution, conversion, or execution fails.</exception>
        public static string Execute(
            string code,
            string inputParameters,
            string types,
            string inputTypeName,
            string outputTypeName,
            string referencedAssemblies,
            string usingStatements)
        {
            EnsureCustomTypeDefinitionsProvided(inputTypeName, types, "InputTypeName");
            EnsureCustomTypeDefinitionsProvided(outputTypeName, types, "OutputTypeName");

            string[] codeToBuild = {
                "using System;" + 
                "using System.Dynamic;" + 
                "using Newtonsoft.Json;" + 
                "using Newtonsoft.Json.Linq;" +
                (usingStatements ?? string.Empty) +
                "namespace SparkCode {" +
                (types ?? string.Empty) +
                "   internal static class CSharpRunner {" +
                "       public static " + outputTypeName + " Run(" + inputTypeName + " input) {" +
                "           " + outputTypeName + " output = default(" + outputTypeName + ");" +
                code +
                "           return output;" +
                "       }" +
                "   }" +
                "}"
            };

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

                throw new Exception(exception);
            }

            Module module = compilerResults.CompiledAssembly.GetModules()[0];
            Type moduleType = module.GetType("SparkCode.CSharpRunner");
            MethodInfo methodInfo = moduleType.GetMethod("Run");
            Type inputType = ResolveRequestedType(inputTypeName, compilerResults.CompiledAssembly);
            Type outputType = ResolveRequestedType(outputTypeName, compilerResults.CompiledAssembly);
            object convertedInput = ConvertInputValue(inputParameters, inputType);
            object objResult = methodInfo.Invoke(null, new object[] { convertedInput });

            return ConvertOutputValue(objResult, outputType);
        }

        /// <summary>
        /// Resolves a requested type name from native aliases or from the compiled dynamic assembly.
        /// </summary>
        /// <param name="typeName">Type name to resolve.</param>
        /// <param name="compiledAssembly">Dynamically compiled assembly that may contain custom types.</param>
        /// <returns>The resolved <see cref="Type"/>.</returns>
        /// <exception cref="Exception">Thrown when the type cannot be resolved.</exception>
        private static Type ResolveRequestedType(string typeName, Assembly compiledAssembly)
        {
            Type nativeType;
            if (TryResolveNativeType(typeName, out nativeType))
            {
                return nativeType;
            }

            Type resolvedType = compiledAssembly.GetType(typeName, false, false);
            if (resolvedType != null)
            {
                return resolvedType;
            }

            resolvedType = compiledAssembly
                .GetTypes()
                .FirstOrDefault(type => string.Equals(type.Name, typeName, StringComparison.Ordinal) || string.Equals(type.FullName, typeName, StringComparison.Ordinal));
            if (resolvedType != null)
            {
                return resolvedType;
            }

            throw new Exception("Type resolution failed for '" + typeName + "'. Provide a valid native type name or include the type in the Types parameter.");
        }

        /// <summary>
        /// Attempts to resolve a native CLR type from known aliases or assembly-qualified type names.
        /// </summary>
        /// <param name="typeName">Type name to resolve.</param>
        /// <param name="resolvedType">Resolved native type when found.</param>
        /// <returns><c>true</c> when a supported native type is resolved; otherwise, <c>false</c>.</returns>
        private static bool TryResolveNativeType(string typeName, out Type resolvedType)
        {
            if (NativeTypeAliases.TryGetValue(typeName, out resolvedType))
            {
                return true;
            }

            resolvedType = Type.GetType(typeName, false, true);
            return resolvedType != null && IsNativeConvertibleType(resolvedType);
        }

        /// <summary>
        /// Determines whether a type can be converted with built-in invariant-culture conversion rules.
        /// </summary>
        /// <param name="type">Type to evaluate.</param>
        /// <returns><c>true</c> when the type is natively convertible; otherwise, <c>false</c>.</returns>
        private static bool IsNativeConvertibleType(Type type)
        {
            if (type == typeof(string) || type == typeof(Guid) || type == typeof(DateTime) || type == typeof(DateTimeOffset) || type == typeof(TimeSpan))
            {
                return true;
            }

            return typeof(IConvertible).IsAssignableFrom(type);
        }

        /// <summary>
        /// Ensures custom type definitions are provided when a requested type is not native.
        /// </summary>
        /// <param name="requestedTypeName">Requested type name.</param>
        /// <param name="typesDefinition">Custom type definitions source.</param>
        /// <param name="parameterName">Parameter name used for error context.</param>
        /// <exception cref="Exception">Thrown when a custom type is requested without definitions.</exception>
        private static void EnsureCustomTypeDefinitionsProvided(string requestedTypeName, string typesDefinition, string parameterName)
        {
            Type nativeType;
            if (TryResolveNativeType(requestedTypeName, out nativeType))
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(typesDefinition))
            {
                return;
            }

            throw new Exception("Type resolution failed for '" + requestedTypeName + "'. Provide a valid native type name or include the type in the Types parameter. Parameter: " + parameterName + ".");
        }

        /// <summary>
        /// Converts the raw input string into the requested input type.
        /// </summary>
        /// <param name="inputValue">Raw input string.</param>
        /// <param name="targetType">Type expected by the generated method signature.</param>
        /// <returns>Converted input object.</returns>
        /// <exception cref="Exception">Thrown when input conversion fails.</exception>
        private static object ConvertInputValue(string inputValue, Type targetType)
        {
            string targetTypeName = targetType.Name;

            if (targetType == typeof(string))
            {
                return inputValue;
            }

            if (string.IsNullOrWhiteSpace(inputValue))
            {
                if (!targetType.IsValueType)
                {
                    return null;
                }

                throw new Exception("Input conversion failed for type '" + targetTypeName + "'. InputParameters is null or empty.");
            }

            try
            {
                if (targetType == typeof(Guid))
                {
                    return Guid.Parse(inputValue);
                }

                if (targetType == typeof(DateTime))
                {
                    return DateTime.Parse(inputValue, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                }

                if (targetType == typeof(DateTimeOffset))
                {
                    return DateTimeOffset.Parse(inputValue, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                }

                if (targetType == typeof(TimeSpan))
                {
                    return TimeSpan.Parse(inputValue, CultureInfo.InvariantCulture);
                }

                if (IsNativeConvertibleType(targetType))
                {
                    return Convert.ChangeType(inputValue, targetType, CultureInfo.InvariantCulture);
                }

                return JsonConvert.DeserializeObject(inputValue, targetType);
            }
            catch (Exception ex)
            {
                throw new Exception("Input conversion failed for type '" + targetTypeName + "' with value '" + inputValue + "'. " + ex.Message);
            }
        }

        /// <summary>
        /// Converts the execution result into a string for API output.
        /// </summary>
        /// <param name="outputValue">Runtime output object returned by the generated method.</param>
        /// <param name="outputType">Declared output type.</param>
        /// <returns>String representation or JSON payload for the output value.</returns>
        /// <exception cref="Exception">Thrown when output conversion fails.</exception>
        private static string ConvertOutputValue(object outputValue, Type outputType)
        {
            string outputTypeName = outputType.Name;

            if (outputValue == null)
            {
                return null;
            }

            if (outputType == typeof(string))
            {
                return Convert.ToString(outputValue, CultureInfo.InvariantCulture);
            }

            try
            {
                if (IsNativeConvertibleType(outputType))
                {
                    return Convert.ToString(outputValue, CultureInfo.InvariantCulture);
                }

                return JsonConvert.SerializeObject(outputValue);
            }
            catch (Exception ex)
            {
                throw new Exception("Output conversion failed for type '" + outputTypeName + "'. " + ex.Message);
            }
        }
    }
}
