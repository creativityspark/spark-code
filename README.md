# Spark Code
Code components for the Power Platform and Dynamics 365.

# How to install
To install these libraries on you environment, install the managed solution available on the releases section of this repo.

# Custom APIs
Dataverse Custom APIs are server-side actions exposed through the Dataverse Web API and SDK. They accept named parameters, run server logic, and return JSON responses.

They require appropriate solution registration and user privileges.

The custom APIs can be used from Power Automate Flows, Classic Workflows or Canvas Apps and Pages and PowerFX functions.

## Power Automate 

To use a custom API in your flow:
1. Include  Dataverse connector (Common Data Service (current environment)):
1. Action: "Perform a bound action" or "Perform an unbound action".
1. Provide the custom API name and JSON input parameters.

## Classic (legacy) Workflows / Processes

Classic workflow designer can call registered Actions via the "Perform Action" step.

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

