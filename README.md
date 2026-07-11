# Spark Code
Code components for the Power Platform and Dynamics 365.

# Installation
Install the managed solution from this repository's Releases page in your target Dataverse environment.

# Custom APIs
Dataverse Custom APIs are server-side actions exposed through the Dataverse Web API and SDK. They accept named parameters, run server logic, and return JSON responses.

These custom APIs can be consumed from Power Automate flows, classic workflows, and Canvas apps/custom pages.

## Power Automate

To use a custom API in your flow:
1. Add the Microsoft Dataverse connector (current environment).
1. Action: "Perform a bound action" or "Perform an unbound action".
1. Provide the custom API name and JSON input parameters.

## Canvas Apps and Pages

In Canvas Apps and Custom Pages:
1. Make a connection to the Environment table of Dataverse.
1. Call the custom API by name.
1. Pass the input parameters.
1. Process the returned results in Power Fx.

## Classic (legacy) Workflows / Processes

Classic workflow designer can call registered Actions via the "Perform Action" step.

# Included Custom APIs

This repository includes several Dataverse Custom APIs. Each entry below shows the class name and a short description.

## Dataverse
- **CalculateRollupField**: Triggers Dataverse rollup field recalculation for a target record.
- **GetViewData**: Retrieves records from a Dataverse saved view by ViewId or by TableName and ViewName.
- **PublishWebResource**: Publishes a Dataverse web resource by name.
- **RunFetchXml**: A plugin that executes a FetchXML query and returns the results.
- **RunSQL**: Executes a SQL query against the Dataverse TDS endpoint and returns the results.

## Data
- **Join**: Joins two JSON arrays by matching key fields and returns merged records.
- **ParseCsv**: Parses CSV data and returns rows as an expando object.
- **Select**: Performs a JSONQuery select on the specified data and returns the results.
- **XmlToJson**: Converts an XML string to JSON.

## Other
- **HttpRequest**: Performs an HTTP request with optional body and `{{paramN}}` placeholder replacement.
- **RunCSharp**: Compiles and executes C# 4.0 code at runtime, supporting optional custom type definitions (`Types`), typed input/output conversion (`InputTypeName` / `OutputTypeName`), input payloads, and custom assembly/usings.

## Templates
- **GetFrontMatter**: Extracts YAML front matter from a text input and returns it together with the remaining body.
- **RenderDataverseTemplate**: Renders a [Liquid](https://github.com/sebastienros/fluid) template by sourcing context values from an optional Dataverse record and optional additional context.
- **RenderTemplate**: Renders a [Liquid](https://github.com/sebastienros/fluid) template using the provided JSON context data.
- **RenderWebResourceTemplate**: Renders a [Liquid](https://github.com/sebastienros/fluid) template stored in a Dataverse web resource by sourcing context values from an optional Dataverse record and optional additional context.

## Text
- **Base64Decode**: Converts a Base64 encoded string into a UTF8 string.
- **Base64Encode**: Converts a UTF8 string into a Base64 encoded string.
- **Parse**: Converts a text value to different data types.
- **ParseURL**: Parses a URL and returns its parts.
- **RegexMatches**: Captures all matches of a regular expression over an input text.
- **RegexReplace**: Runs a regular expression replacement over the provided input text.
- **ReplaceParams**: Replaces parameter placeholders in text with provided values.

# Security Notes
Take these recommendations into account when using the included custom APIs:

- **RunSQL**: Keep SQL inputs restricted and trusted. Avoid exposing this API to broad caller scopes.
- **RunCSharp**: Treat dynamic code execution as privileged functionality. Restrict who can invoke it.
- **HttpRequest**: Only allow trusted outbound destinations and carefully validate request parameters.
- Apply least-privilege principles to all users, service principals, and app roles that can execute custom APIs.


# Contributor Workflow

Take a look at this section if you want to contribute to this repository.

## Prerequisites
- A Dataverse environment where you can install managed solutions.
- Permissions to import solutions and execute custom APIs.
- .NET SDK installed and available in PATH.
- `DATAVERSE_CONNECTION_STRING_SPARK_CODE` environment variable set when running deployment scripts or integration tests.

## Build and deploy SparkCode.API

- A tool is provided to build and deploy the `SparkCode.API` assembly and automatically register the included custom APIs in a Dataverse environment. The tool uses the headers xml documentation to understand the input/output parameters of each API and register them accordingly.

- To run the deployment tool:
-- Build the solution in either Debug or Release configuration.
-- Set the `DATAVERSE_CONNECTION_STRING_SPARK_CODE` environment variable to a valid connection string for your target Dataverse environment. Use this powershell command to register the environment variable:
`[System.Environment]::SetEnvironmentVariable('DATAVERSE_CONNECTION_STRING_SPARK_CODE', 'AuthType=ClientSecret;Url=https://url.crm.dynamics.com;AppId=XXX;ClientSecret=YYY', 'User')`
-- Run the SparkCode.APIRegistrationTool to register the APIs in your target Dataverse environment.

## Test

- Before running the tests, run the deployment tool to ensure the APIs are registered in your target Dataverse environment. Some of the unit tests will connect to your environment to verify the APIs are working as expected.

- Make sure the `DATAVERSE_CONNECTION_STRING_TESTS` environment variable is set to a valid connection string for your target Dataverse environment. Use this powershell command to register the environment variable:
`[System.Environment]::SetEnvironmentVariable('DATAVERSE_CONNECTION_STRING_TESTS', 'AuthType=ClientSecret;Url=https://url.crm.dynamics.com;AppId=XXX;ClientSecret=YYY', 'User')`

- Unit tests: Hits the internal logic of the custom APIs.
	- `dotnet test ./src/assemblies/SparkCode.Tests/SparkCode.Tests.csproj`

- Integration tests: Hits the registered custom APIs in a Dataverse environment.
	- `dotnet test ./src/assemblies/SparkCode.API.Tests/SparkCode.API.Tests.csproj`

## Architecture

The main goal of the architecture used in this solution is to maximize simplicity and reusability, allowing every custom API to be used in as many places as possible.

We also include unit tests. To make testing easier, we separate the core logic from custom API plumbing code.

This approach minimizes duplicated code.

Solution structure:
```
SparkCode:              Core logic of every custom API.
SparkCode.Tests:        Unit tests of the core logic.
SparkCode.API:          API plumbing code. This library exposes the core logic to Dataverse.
SparkCode.API.Tests:    Integration tests. Performs test calls to a Dataverse environment where the custom APIs are installed.
```
This library is compiled as a [plugin package](https://learn.microsoft.com/en-us/power-apps/developer/data-platform/build-and-package#all-projects-must-be-in-the-sdk-style) so additional dependencies can be included when needed.

Custom APIs can return [Expando](https://learn.microsoft.com/en-us/power-apps/developer/data-platform/webapi/reference/expando?view=dataverse-latest) objects. Compared to plain JSON text, Expando output in Power Automate usually avoids extra parsing because the editor recognizes the returned structure directly.

However, APIs that use Expando output cannot be used in classic workflows. To keep compatibility, some APIs are registered twice: one Expando version and one `Json`-suffixed version that returns JSON text.

## Troubleshooting
- If deployment fails, verify `DATAVERSE_CONNECTION_STRING_SPARK_CODE` is set in the current shell/user context.
- If custom actions are not visible in Power Automate, confirm the managed solution is imported in the same target environment.
- If integration tests fail, confirm APIs are deployed and that test credentials have permission to execute them.
- If registration fails, rerun the deployment script in the same configuration (Debug/Release) for both projects.