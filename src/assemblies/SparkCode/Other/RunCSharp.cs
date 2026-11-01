using Microsoft.CSharp;
using Newtonsoft.Json;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Text;

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
        /// <returns>Output, complete logs, and structured compiler or runtime diagnostics.</returns>
        /// <exception cref="Exception">Thrown for invalid type configuration or infrastructure failures.</exception>
        public static RunCSharpResult Execute(
            string code,
            string inputParameters,
            string types,
            string inputTypeName,
            string outputTypeName,
            string referencedAssemblies,
            string usingStatements)
        {
            var result = new RunCSharpResult();
            inputTypeName = string.IsNullOrWhiteSpace(inputTypeName) ? "string" : inputTypeName;
            outputTypeName = string.IsNullOrWhiteSpace(outputTypeName) ? "string" : outputTypeName;

            EnsureCustomTypeDefinitionsProvided(inputTypeName, types, "InputTypeName");
            EnsureCustomTypeDefinitionsProvided(outputTypeName, types, "OutputTypeName");

            var source = new StringBuilder();
            source.AppendLine("using System;");
            source.AppendLine("using System.Collections.Generic;");
            source.AppendLine("using System.Dynamic;");
            source.AppendLine("using Newtonsoft.Json;");
            source.AppendLine("using Newtonsoft.Json.Linq;");
            AppendSection(source, usingStatements, "UsingStatements");
            source.AppendLine("namespace SparkCode {");
            AppendSection(source, types, "Types");
            source.AppendLine("internal static class CSharpRunner {");
            source.AppendLine("private static readonly List<string> __logs = new List<string>();");
            source.AppendLine("public static void Log(string message) { __logs.Add(message); }");
            source.AppendLine("public static List<string> GetLogs() { return __logs; }");
            source.AppendLine("public static " + outputTypeName + " Run(" + inputTypeName + " input) {");
            source.AppendLine(outputTypeName + " output = default(" + outputTypeName + ");");
            int codeSourceLine = source.ToString().Count(character => character == '\n') + 1;
            AppendSection(source, code, "Code");
            source.AppendLine("return output;");
            source.AppendLine("}");
            source.AppendLine("}");
            source.AppendLine("}");

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
                IncludeDebugInformation = true,
                CompilerOptions = "/optimize-"
            };

            compilerParameters.ReferencedAssemblies.AddRange(listReferencedAssemblies.ToArray());

            using (var cSharpCodeProvider = new CSharpCodeProvider())
            using (compilerParameters.TempFiles)
            {
                CompilerResults compilerResults = cSharpCodeProvider.CompileAssemblyFromSource(compilerParameters, source.ToString());
                if (compilerResults.Errors.HasErrors)
                {
                    foreach (CompilerError error in compilerResults.Errors)
                    {
                        if (error.IsWarning)
                        {
                            continue;
                        }

                        string section = GetSection(error.FileName);
                        if (section == null && (error.Line < codeSourceLine || string.IsNullOrEmpty(error.FileName)))
                        {
                            throw new InvalidOperationException("Compiler configuration or generated-source failure: " + error);
                        }

                        result.CompileErrors.Add(new CompileDiagnostic
                        {
                            // Incomplete user blocks can be diagnosed at a generated closing brace.
                            Line = section == null ? null : (int?)error.Line,
                            Column = section == null ? null : (int?)error.Column,
                            Code = error.ErrorNumber,
                            Message = error.ErrorText,
                            Section = section
                        });
                    }
                    return result;
                }
                return ExecuteCompiled(compilerResults.CompiledAssembly, inputParameters, inputTypeName, outputTypeName, result);
            }
        }

        private static RunCSharpResult ExecuteCompiled(
            Assembly assembly, string inputParameters, string inputTypeName, string outputTypeName, RunCSharpResult result)
        {
            Module module = assembly.GetModules()[0];
            Type moduleType = module.GetType("SparkCode.CSharpRunner");
            MethodInfo methodInfo = moduleType.GetMethod("Run");
            Type inputType = ResolveRequestedType(inputTypeName, assembly);
            Type outputType = ResolveRequestedType(outputTypeName, assembly);
            MethodInfo getLogsMethod = moduleType.GetMethod("GetLogs");
            object convertedInput;
            try
            {
                convertedInput = ConvertInputValue(inputParameters, inputType);
            }
            catch (Exception exception) when (
                exception is JsonException || exception is FormatException || exception is OverflowException ||
                exception is InvalidCastException || exception is ArgumentException || exception is TargetInvocationException)
            {
                var diagnostic = CreateRuntimeDiagnostic(exception, "InputParameters", false);
                var readerException = exception as JsonReaderException;
                var serializationException = exception as JsonSerializationException;
                int line = readerException != null ? readerException.LineNumber
                    : serializationException != null ? serializationException.LineNumber : 0;
                diagnostic.Line = line > 0 ? (int?)line : null;
                result.Logs = JoinLogs(getLogsMethod);
                result.RuntimeErrors.Add(diagnostic);
                return result;
            }

            object objResult;
            try
            {
                objResult = methodInfo.Invoke(null, new object[] { convertedInput });
            }
            catch (TargetInvocationException ex)
            {
                result.Logs = JoinLogs(getLogsMethod);
                result.RuntimeErrors.Add(CreateRuntimeDiagnostic(ex.InnerException ?? ex, "Code", true));
                return result;
            }

            result.Logs = JoinLogs(getLogsMethod);
            try
            {
                result.Outputs = ConvertOutputValue(objResult, outputType);
            }
            catch (Exception exception) when (
                exception is JsonException || exception is FormatException || exception is InvalidCastException ||
                exception is TargetInvocationException)
            {
                result.RuntimeErrors.Add(CreateRuntimeDiagnostic(exception, "Code", false));
            }
            result.Logs = JoinLogs(getLogsMethod);
            return result;
        }

        private static void AppendSection(StringBuilder source, string text, string section)
        {
            source.AppendLine("#line 1 \"" + section + ".cs\"");
            source.AppendLine((text ?? string.Empty).Replace("\r\n", "\n").Replace('\r', '\n'));
            source.AppendLine("#line default");
        }

        private static string GetSection(string fileName)
        {
            string name = System.IO.Path.GetFileNameWithoutExtension(fileName ?? string.Empty);
            return name == "Code" || name == "Types" || name == "UsingStatements" ? name : null;
        }

        private static RuntimeDiagnostic CreateRuntimeDiagnostic(Exception exception, string section, bool mapSource)
        {
            var diagnostic = new RuntimeDiagnostic
            {
                Section = section,
                Message = exception.Message,
                ExceptionType = exception.GetType().FullName,
                StackTrace = exception.StackTrace
            };
            if (mapSource)
            {
                bool foundUserFrame = false;
                foreach (StackFrame frame in new StackTrace(exception, true).GetFrames() ?? new StackFrame[0])
                {
                    string mappedSection = GetSection(frame.GetFileName());
                    if (mappedSection != null && frame.GetFileLineNumber() > 0)
                    {
                        diagnostic.Section = mappedSection;
                        diagnostic.Line = frame.GetFileLineNumber();
                        break;
                    }
                    var declaringType = frame.GetMethod().DeclaringType;
                    if (!foundUserFrame && declaringType != null &&
                        declaringType.Namespace == "SparkCode" && declaringType.Assembly != typeof(RunCSharp).Assembly)
                    {
                        diagnostic.Section = declaringType.Name == "CSharpRunner" ? "Code" : "Types";
                        foundUserFrame = true;
                    }
                }
            }
            return diagnostic;
        }

        /// <summary>
        /// Reads the messages collected by the generated <c>Log</c> method.
        /// </summary>
        /// <param name="getLogsMethod">Generated <c>GetLogs</c> method.</param>
        /// <returns>Messages separated by new lines, or <c>null</c> when nothing was logged.</returns>
        private static string JoinLogs(MethodInfo getLogsMethod)
        {
            var collected = getLogsMethod.Invoke(null, null) as List<string>;
            if (collected == null || collected.Count == 0)
            {
                return null;
            }

            return string.Join("\n", collected);
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

                throw new ArgumentException("InputParameters is null or empty for value type '" + targetTypeName + "'.", "InputParameters");
            }

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

        /// <summary>
        /// Converts the execution result into a string for API output.
        /// </summary>
        /// <param name="outputValue">Runtime output object returned by the generated method.</param>
        /// <param name="outputType">Declared output type.</param>
        /// <returns>String representation or JSON payload for the output value.</returns>
        /// <exception cref="Exception">Thrown when output conversion fails.</exception>
        private static string ConvertOutputValue(object outputValue, Type outputType)
        {
            if (outputValue == null)
            {
                return null;
            }

            if (outputType == typeof(string))
            {
                return Convert.ToString(outputValue, CultureInfo.InvariantCulture);
            }

            if (IsNativeConvertibleType(outputType))
            {
                return Convert.ToString(outputValue, CultureInfo.InvariantCulture);
            }

            return JsonConvert.SerializeObject(outputValue);
        }
    }

    public sealed class RunCSharpResult
    {
        public string Outputs { get; set; }
        public string Logs { get; set; }
        public List<CompileDiagnostic> CompileErrors { get; } = new List<CompileDiagnostic>();
        public List<RuntimeDiagnostic> RuntimeErrors { get; } = new List<RuntimeDiagnostic>();
    }

    public sealed class CompileDiagnostic
    {
        [JsonProperty("line")]
        public int? Line { get; set; }
        [JsonProperty("column")]
        public int? Column { get; set; }
        [JsonProperty("code")]
        public string Code { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("section")]
        public string Section { get; set; }
    }

    public sealed class RuntimeDiagnostic
    {
        [JsonProperty("line")]
        public int? Line { get; set; }
        [JsonProperty("section")]
        public string Section { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("exceptionType")]
        public string ExceptionType { get; set; }
        [JsonProperty("stackTrace")]
        public string StackTrace { get; set; }
    }
}
