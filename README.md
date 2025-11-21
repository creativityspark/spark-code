# Spark Code
Code components for the Power Platform and Dynamics 365.

# How to install
To install these libraries on you environment, install the managed solution available on the releases section of this repo.

# Custom APIs
Dataverse Custom APIs are server-side actions (bound or unbound) exposed through the Dataverse Web API and SDK. They accept named parameters, run server logic, and return JSON responses.

They require appropriate solution registration and user privileges.

The custom APIs can be used from Power Automate Flows, Classic Workflows or Canvas Apps and Pages and PowerFX functions.

## Power Automate 

To use a custom API in your flow:
1. Include  Dataverse connector (Common Data Service (current environment)):
1. Action: "Perform a bound action" or "Perform an unbound action".
1. Provide the custom API name and JSON input parameters.

## Classic (legacy) Workflows / Processes

Classic workflow designer can call registered Actions (SDK Actions) via the "Perform Action" step.

## Included Custom APIs

The repository includes several Dataverse Custom APIs (server-side plugins) located in `src/assemblies/SparkCode.CustomAPIs`. Each entry below shows the class name and a short description.

- **Base64Encode**: Encodes an input string to Base64. (`src/assemblies/SparkCode.CustomAPIs/Base64Encode.cs`)
- **Base64Decode**: Decodes a Base64-encoded input string. (`src/assemblies/SparkCode.CustomAPIs/Base64Decode.cs`)
- **CsvToJson**: Converts CSV text into a JSON array (supports delimiter and quoted fields). (`src/assemblies/SparkCode.CustomAPIs/CsvToJson.cs`)
- **XmlToJson**: Converts an XML string into JSON. (`src/assemblies/SparkCode.CustomAPIs/XmlToJson.cs`)
- **JsonJoin**: Joins two JSON arrays on specified keys and returns the joined result. (`src/assemblies/SparkCode.CustomAPIs/JsonJoin.cs`)
- **JSonSelect**: Selects token(s) from JSON using JSONPath-like queries. (`src/assemblies/SparkCode.CustomAPIs/JsonSelect.cs`)
- **ParseURL**: Parses a URL to extract `etc` (object type code) and `id`, and resolves the logical type name. (`src/assemblies/SparkCode.CustomAPIs/ParseURL.cs`)
- **RegexMatch**: Runs a regex match and returns success, index and matched value. (`src/assemblies/SparkCode.CustomAPIs/RegexMatch.cs`)
- **RegexMatches**: Finds all regex matches and returns capture info as JSON. (`src/assemblies/SparkCode.CustomAPIs/RegexMatches.cs`)
- **RegexReplace**: Performs regex replacements on a string. (`src/assemblies/SparkCode.CustomAPIs/RegexReplace.cs`)
- **HttpRequest**: Performs HTTP requests with optional parameter substitution and returns the response body. (`src/assemblies/SparkCode.CustomAPIs/HttpRequest.cs`)
- **RunCSharp**: Compiles and runs supplied C# code (CodeDom) and returns execution output. (`src/assemblies/SparkCode.CustomAPIs/RunCSharp.cs`)
- **RunSQL**: Executes SQL queries against an external SQL endpoint using token acquisition (managed identity) and returns JSON results. (`src/assemblies/SparkCode.CustomAPIs/RunSQL.cs`)
- **RunFetchXmlQuery**: Executes a FetchXML query in Dataverse and returns results as JSON. (`src/assemblies/SparkCode.CustomAPIs/RunFetchXmlQuery.cs`)
- **GetFrontMatter**: Extracts YAML front matter from text and returns it as JSON plus the remaining body. (`src/assemblies/SparkCode.CustomAPIs/GetFrontMatter.cs`)
- **GetViewData**: Retrieves data for a Saved View and returns it as JSON (option for friendly column names). (`src/assemblies/SparkCode.CustomAPIs/GetViewData.cs`)
- **UpdateRollupField**: Triggers calculation of a rollup field on a specified record. (`src/assemblies/SparkCode.CustomAPIs/UpdateRollupField.cs`)


# Copyright
Some of the code included in these libraries is taken from [Guido Preite's github repo](https://github.com/GuidoPreite/).

