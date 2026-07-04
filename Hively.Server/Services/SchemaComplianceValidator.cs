using System.Text.Json;

namespace Hively.Server.Services
{
    /// <summary>
    /// Pure function validating a topic's last-seen payload against its assigned
    /// schema's definition. The definition mirrors the payload's own shape, with
    /// leaf values replaced by type names ("string"/"number"/"boolean", or
    /// "object"/"array"/"null" to assert a field's JSON kind without checking
    /// what's inside it) — a nested object in the definition means "recurse and
    /// check these specific sub-fields" instead. A trailing '?' on a leaf type
    /// (e.g. "string?") marks the field optional: absent or explicitly null is
    /// fine, but a present, non-null value must still match the base type.
    /// </summary>
    public static class SchemaComplianceValidator
    {
        /// <summary>
        /// Returns (null, []) when no schema is assigned — compliance is unknown, not
        /// pass/fail. Returns (false, [...]) when a schema is assigned but no payload
        /// has been seen yet. Otherwise checks every field in the schema definition
        /// against the payload, collecting every mismatch rather than short-circuiting.
        /// </summary>
        public static (bool? Compliant, List<string> Mismatches) Validate(string? schemaDefinitionJson, string? payloadJson)
        {
            if (schemaDefinitionJson == null)
            {
                return (null, new List<string>());
            }

            if (payloadJson == null)
            {
                return (false, new List<string> { "No message received yet" });
            }

            using var definitionDocument = JsonDocument.Parse(schemaDefinitionJson);
            using var payloadDocument = JsonDocument.Parse(payloadJson);

            var mismatches = new List<string>();
            ValidateFields(definitionDocument.RootElement, payloadDocument.RootElement, path: "", mismatches);

            return (mismatches.Count == 0, mismatches);
        }

        private static void ValidateFields(JsonElement definition, JsonElement payload, string path, List<string> mismatches)
        {
            foreach (var property in definition.EnumerateObject())
            {
                var fieldPath = path.Length == 0 ? property.Name : $"{path}.{property.Name}";
                var value = default(JsonElement);
                var hasValue = payload.ValueKind == JsonValueKind.Object && payload.TryGetProperty(property.Name, out value);

                if (property.Value.ValueKind == JsonValueKind.Object)
                {
                    // Nested schema: the field itself must be an object, then its
                    // sub-fields are checked against this nested definition. Nested
                    // objects are always required — '?' only applies to leaf types.
                    if (!hasValue)
                    {
                        mismatches.Add($"'{fieldPath}' missing");
                        continue;
                    }

                    if (value.ValueKind != JsonValueKind.Object)
                    {
                        mismatches.Add($"'{fieldPath}' expected object, got {DescribeType(value)}");
                        continue;
                    }

                    ValidateFields(property.Value, value, fieldPath, mismatches);
                }
                else if (property.Value.ValueKind == JsonValueKind.String)
                {
                    var declaredType = property.Value.GetString()!;
                    var optional = declaredType.EndsWith('?');
                    var expectedType = optional ? declaredType[..^1] : declaredType;

                    if (!hasValue)
                    {
                        if (!optional)
                        {
                            mismatches.Add($"'{fieldPath}' missing");
                        }
                        continue;
                    }

                    if (optional && value.ValueKind == JsonValueKind.Null)
                    {
                        continue;
                    }

                    if (!MatchesType(value, expectedType))
                    {
                        mismatches.Add($"'{fieldPath}' expected {expectedType}, got {DescribeType(value)}");
                    }
                }
                // Any other declared shape (number/bool/array/null literal as the
                // definition value itself) is a schema-authoring mistake, not a
                // payload compliance failure — skip rather than guess intent.
            }
        }

        private static bool MatchesType(JsonElement value, string expectedType) => expectedType switch
        {
            "number" => value.ValueKind == JsonValueKind.Number,
            "string" => value.ValueKind == JsonValueKind.String,
            "boolean" => value.ValueKind is JsonValueKind.True or JsonValueKind.False,
            "object" => value.ValueKind == JsonValueKind.Object,
            "array" => value.ValueKind == JsonValueKind.Array,
            "null" => value.ValueKind == JsonValueKind.Null,
            _ => true // unrecognized declared type — a schema-authoring issue, not a payload compliance failure
        };

        private static string DescribeType(JsonElement value) => value.ValueKind switch
        {
            JsonValueKind.Number => "number",
            JsonValueKind.String => "string",
            JsonValueKind.True or JsonValueKind.False => "boolean",
            JsonValueKind.Null => "null",
            JsonValueKind.Object => "object",
            JsonValueKind.Array => "array",
            _ => value.ValueKind.ToString().ToLowerInvariant()
        };
    }
}
