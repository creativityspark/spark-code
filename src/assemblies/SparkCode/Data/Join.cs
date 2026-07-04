using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace SparkCode.Data
{
    /// <summary>
    /// Provides utilities to perform inner joins between two JSON arrays of objects.
    /// </summary>
    public static class Join
    {
        /// <summary>
        /// Represents the Join output with Dataverse-safe entity rows and JSON rows.
        /// </summary>
        public sealed class JoinResult
        {
            /// <summary>
            /// Joined rows represented as Dataverse entities using underscore-prefixed linked field names.
            /// </summary>
            public EntityCollection Results { get; set; }

            /// <summary>
            /// Joined rows represented as JSON using dotted linked field names.
            /// </summary>
            public string ResultsJson { get; set; }
        }

        /// <summary>
        /// Joins two JSON arrays by matching values from <paramref name="field1"/> and <paramref name="field2"/>.
        /// </summary>
        /// <param name="list1">JSON array string containing the left collection.</param>
        /// <param name="list2">JSON array string containing the right collection.</param>
        /// <param name="field1">Join key field name in <paramref name="list1"/>.</param>
        /// <param name="field2">Join key field name in <paramref name="list2"/>.</param>
        /// <returns>An <see cref="EntityCollection"/> with joined rows.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any required input is null, empty, or whitespace.</exception>
        /// <exception cref="JsonReaderException">Thrown when input JSON is invalid or not an array of objects.</exception>
        public static EntityCollection JoinCollections(string list1, string list2, string field1, string field2)
        {
            return JoinCollectionsWithJson(list1, list2, field1, field2).Results;
        }

        /// <summary>
        /// Joins two JSON arrays and returns both Dataverse entity output and JSON output.
        /// </summary>
        /// <param name="list1">JSON array string containing the left collection.</param>
        /// <param name="list2">JSON array string containing the right collection.</param>
        /// <param name="field1">Join key field name in <paramref name="list1"/>.</param>
        /// <param name="field2">Join key field name in <paramref name="list2"/>.</param>
        /// <returns>A <see cref="JoinResult"/> containing <c>Results</c> and <c>ResultsJson</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any required input is null, empty, or whitespace.</exception>
        /// <exception cref="JsonReaderException">Thrown when input JSON is invalid or not an array of objects.</exception>
        public static JoinResult JoinCollectionsWithJson(string list1, string list2, string field1, string field2)
        {
            if (string.IsNullOrWhiteSpace(list1))
            {
                throw new ArgumentNullException(nameof(list1));
            }

            if (string.IsNullOrWhiteSpace(list2))
            {
                throw new ArgumentNullException(nameof(list2));
            }

            if (string.IsNullOrWhiteSpace(field1))
            {
                throw new ArgumentNullException(nameof(field1));
            }

            if (string.IsNullOrWhiteSpace(field2))
            {
                throw new ArgumentNullException(nameof(field2));
            }

            var leftRows = ParseArrayOfObjects(list1, nameof(list1));
            var rightRows = ParseArrayOfObjects(list2, nameof(list2));

            var rightLookup = BuildLookup(rightRows, field2);
            var output = new EntityCollection();
            var outputJsonRows = new List<Dictionary<string, object>>();

            foreach (var leftRow in leftRows)
            {
                var leftKey = GetRowValue(leftRow, field1);
                if (leftKey == null)
                {
                    continue;
                }

                var normalizedLeftKey = NormalizeKey(leftKey);
                if (string.IsNullOrWhiteSpace(normalizedLeftKey))
                {
                    continue;
                }

                if (!rightLookup.TryGetValue(normalizedLeftKey, out var matches))
                {
                    continue;
                }

                foreach (var rightRow in matches)
                {
                    var joined = new Entity();
                    var joinedJsonRow = new Dictionary<string, object>(StringComparer.Ordinal);

                    foreach (var kvp in leftRow)
                    {
                        joined[kvp.Key] = kvp.Value;
                        joinedJsonRow[kvp.Key] = kvp.Value;
                    }

                    foreach (var kvp in rightRow)
                    {
                        var resultsFieldName = BuildResultsFieldName(field1, kvp.Key);
                        if (!joined.Attributes.Contains(resultsFieldName))
                        {
                            joined[resultsFieldName] = kvp.Value;
                        }

                        var jsonFieldName = BuildJsonFieldName(field1, kvp.Key);
                        if (!joinedJsonRow.ContainsKey(jsonFieldName))
                        {
                            joinedJsonRow[jsonFieldName] = kvp.Value;
                        }
                    }

                    output.Entities.Add(joined);
                    outputJsonRows.Add(joinedJsonRow);
                }
            }

            return new JoinResult
            {
                Results = output,
                ResultsJson = SerializeRows(outputJsonRows)
            };
        }

        private static string BuildResultsFieldName(string field1, string fieldName)
        {
            return string.Concat(field1, "_", fieldName);
        }

        private static string BuildJsonFieldName(string field1, string fieldName)
        {
            return string.Concat(field1, ".", fieldName);
        }

        private static List<Dictionary<string, object>> ParseArrayOfObjects(string json, string parameterName)
        {
            var token = JToken.Parse(json);
            if (!(token is JArray array))
            {
                throw new JsonReaderException(string.Concat(parameterName, " must be a JSON array."));
            }

            var rows = new List<Dictionary<string, object>>();

            foreach (var item in array)
            {
                if (!(item is JObject obj))
                {
                    throw new JsonReaderException(string.Concat(parameterName, " must contain JSON objects only."));
                }

                var row = new Dictionary<string, object>(StringComparer.Ordinal);
                foreach (var property in obj.Properties())
                {
                    row[property.Name] = ConvertTokenValue(property.Value);
                }

                rows.Add(row);
            }

            return rows;
        }

        private static Dictionary<string, List<Dictionary<string, object>>> BuildLookup(IEnumerable<Dictionary<string, object>> rows, string keyField)
        {
            var lookup = new Dictionary<string, List<Dictionary<string, object>>>(StringComparer.Ordinal);

            foreach (var row in rows)
            {
                var keyValue = GetRowValue(row, keyField);
                if (keyValue == null)
                {
                    continue;
                }

                var normalized = NormalizeKey(keyValue);
                if (string.IsNullOrWhiteSpace(normalized))
                {
                    continue;
                }

                if (!lookup.TryGetValue(normalized, out var bucket))
                {
                    bucket = new List<Dictionary<string, object>>();
                    lookup[normalized] = bucket;
                }

                bucket.Add(row);
            }

            return lookup;
        }

        private static object GetRowValue(Dictionary<string, object> row, string fieldName)
        {
            if (!row.TryGetValue(fieldName, out var value))
            {
                return null;
            }

            return value;
        }

        private static string NormalizeKey(object value)
        {
            if (value == null)
            {
                return null;
            }

            if (value is string stringValue)
            {
                return stringValue;
            }

            if (value is IFormattable formattable)
            {
                return formattable.ToString(null, CultureInfo.InvariantCulture);
            }

            return value.ToString();
        }

        private static object ConvertTokenValue(JToken token)
        {
            if (token == null || token.Type == JTokenType.Null || token.Type == JTokenType.Undefined)
            {
                return null;
            }

            switch (token.Type)
            {
                case JTokenType.Integer:
                    var integer = token.Value<long>();
                    if (integer >= int.MinValue && integer <= int.MaxValue)
                    {
                        return (int)integer;
                    }

                    return integer;
                case JTokenType.Float:
                    return token.Value<double>();
                case JTokenType.Boolean:
                    return token.Value<bool>();
                case JTokenType.Date:
                    return token.Value<DateTime>();
                case JTokenType.String:
                    return token.Value<string>();
                default:
                    return token.ToString(Formatting.None);
            }
        }

        private static string SerializeRows(List<Dictionary<string, object>> rows)
        {
            using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            using (var jsonWriter = new JsonTextWriter(stringWriter))
            {
                jsonWriter.WriteStartArray();

                foreach (var row in rows)
                {
                    jsonWriter.WriteStartObject();
                    foreach (var kvp in row)
                    {
                        jsonWriter.WritePropertyName(kvp.Key);
                        if (kvp.Value == null)
                        {
                            jsonWriter.WriteNull();
                        }
                        else
                        {
                            jsonWriter.WriteValue(kvp.Value);
                        }
                    }

                    jsonWriter.WriteEndObject();
                }

                jsonWriter.WriteEndArray();
                return stringWriter.ToString();
            }
        }
    }
}
