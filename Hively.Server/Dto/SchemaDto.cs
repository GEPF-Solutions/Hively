using Hively.Server.DbModel;

namespace Hively.Server.Dto
{
    public class SchemaDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;

        /// <summary>Field -&gt; type map as a JSON string, e.g. {"value":"number","unit":"string"}.</summary>
        public string Definition { get; set; } = null!;

        /// <summary>System-managed, e.g. "v1", "v2" — ignored on insert/update, set by the service. Nullable so the client isn't required to send it.</summary>
        public string? Version { get; set; }

        public SchemaDto(Schema schema)
        {
            Id = schema.Id;
            Name = schema.Name;
            Definition = schema.Definition;
            Version = schema.Version;
        }

        // Empty constructor for deserialization
        public SchemaDto()
        {
        }
    }
}
