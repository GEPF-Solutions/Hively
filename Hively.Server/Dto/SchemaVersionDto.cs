using Hively.Server.DbModel;

namespace Hively.Server.Dto
{
    public class SchemaVersionDto
    {
        public Guid Id { get; set; }
        public string Version { get; set; } = null!;
        public string Definition { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        public SchemaVersionDto(SchemaVersion schemaVersion)
        {
            Id = schemaVersion.Id;
            Version = schemaVersion.Version;
            Definition = schemaVersion.Definition;
            CreatedAt = schemaVersion.CreatedAt;
        }

        // Empty constructor for deserialization
        public SchemaVersionDto()
        {
        }
    }
}
