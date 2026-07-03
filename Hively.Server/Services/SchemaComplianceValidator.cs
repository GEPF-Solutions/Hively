using System.Text.Json;

namespace Hively.Server.Services
{
    /// <summary>
    /// Pure function validating a topic's last-seen payload against its assigned
    /// schema's field-&gt;type definition. Ported from the prototype's validateSchema.
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

            var definition = JsonSerializer.Deserialize<Dictionary<string, string>>(schemaDefinitionJson)
                ?? new Dictionary<string, string>();
            using var payloadDocument = JsonDocument.Parse(payloadJson);
            var payload = payloadDocument.RootElement;

            var mismatches = new List<string>();
            foreach (var (field, expectedType) in definition)
            {
                if (!payload.TryGetProperty(field, out var value))
                {
                    mismatches.Add($"'{field}' missing");
                    continue;
                }

                if (!MatchesType(value, expectedType))
                {
                    mismatches.Add($"'{field}' expected {expectedType}, got {DescribeType(value)}");
                }
            }

            return (mismatches.Count == 0, mismatches);
        }

        private static bool MatchesType(JsonElement value, string expectedType) => expectedType switch
        {
            "number" => value.ValueKind == JsonValueKind.Number,
            "string" => value.ValueKind == JsonValueKind.String,
            "boolean" => value.ValueKind is JsonValueKind.True or JsonValueKind.False,
            _ => true // unrecognized declared type — a schema-authoring issue, not a payload compliance failure
        };

        private static string DescribeType(JsonElement value) => value.ValueKind switch
        {
            JsonValueKind.Number => "number",
            JsonValueKind.String => "string",
            JsonValueKind.True or JsonValueKind.False => "boolean",
            JsonValueKind.Null => "null",
            _ => value.ValueKind.ToString().ToLowerInvariant()
        };
    }
}
