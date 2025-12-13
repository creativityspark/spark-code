# Spark Code
Code components for the Power Platform and Dynamics 365.

# How to install
To install these libraries on your environment, install the managed solution available on the releases section of this repo.

# Custom APIs
Dataverse Custom APIs are server-side actions exposed through the Dataverse Web API and SDK. They accept named parameters, run server logic, and return JSON responses.

The custom APIs can be consumed from Power Automate Flows, Classic Workflows or Canvas Apps and Pages.

## Power Automate 

To use a custom API in your flow:
1. Include  Dataverse connector (Common Data Service (current environment)):
1. Action: "Perform a bound action" or "Perform an unbound action".
1. Provide the custom API name and JSON input parameters.

## Canvas Apps and Pages

In Canvas Apps and Custom Pages:
1. Make a connection to the Environment table of Dataverse.
1. Call the Custom API by name. 
1. Pass the input parameters.
1. Process or use the returned results using PowerFX code.

## Classic (legacy) Workflows / Processes

Classic workflow designer can call registered Actions via the "Perform Action" step.

## Architecture

The main goal of the architecture used in this solution is to maximize simplicity and reusability, allowing every custom API to be used in as many places as possible.

Another goal is to included tests, and for this purpose, to separate logic from Custom API plumbing code.

All of the above, minimizing duplicated code.

Custom APIs allow returning [Expando](https://learn.microsoft.com/en-us/power-apps/developer/data-platform/webapi/reference/expando?view=dataverse-latest) objects as return values. The main advantage of this data type over plain JSON values is that when used in a Power Automate flow, it doesn't require a extra parsing operation; the structure of the returned data is understood by the Power Automate editor, and immediately available.

However, when an Expando object is returned, the API can't be made available to Classic Workflows. That's why in some cases we have the same API registered twice, one that will make use of the Expando object output, and another, with a "_Json" suffix that will return the same values as JSON format instead, to keep the compatibility with classic Workflows.  

## Included Custom APIs

This repository includes several Dataverse Custom APIs. Each entry below shows the class name and a short description.

### Dataverse
- **GetViewData**: Retrieves data for a Saved View and returns it as JSON.
- **RunFetchXml**: Executes a FetchXML query in Dataverse and returns results as JSON.
- **RunSQL**: Executes SQL queries against an external SQL endpoint and returns JSON results.
- **UpdateRollupField**: Triggers calculation of a rollup field on a specified record.

### Data
- **CsvToJson**: Converts CSV text into a JSON array.
- **JsonJoin**: Joins two JSON arrays on specified keys and returns the joined result.
- **Select**: Selects token(s) from JSON using JSONPath-like queries.
- **XmlToJson**: Converts an XML string into JSON.

### Other
- **HttpRequest**: Performs HTTP requests with optional parameter substitution and returns the response body.
- **ParseURL**: Parses a URL to extract `etc` (object type code) and `id`, and resolves the logical type name.
- **RunCSharp**: Compiles and runs supplied C# code and returns execution output.

### Templates
- **GetFrontMatter**: Extracts YAML front matter from text and returns it as JSON plus the remaining body.

### Text
- **Base64Decode**: Decodes a Base64-encoded input string.
- **Base64Encode**: Encodes an input string to Base64.
- **RegexMatch**: Runs a regex match and returns success, index and matched value.
- **RegexMatches**: Finds all regex matches and returns capture info as JSON.
- **RegexReplace**: Performs regex replacements on a string.

# Copyright
Some of the code included in these libraries is taken from [Guido Preite's github repo](https://github.com/GuidoPreite/).

