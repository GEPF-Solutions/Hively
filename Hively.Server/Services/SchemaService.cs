using Hively.Server.Dto;
using Hively.Server.Repository.Abstractions;
using Hively.Server.Services.Abstractions;

namespace Hively.Server.Services
{
    /// <summary>
    /// Service for managing Schema entities and their version history.
    /// </summary>
    public class SchemaService : ISchemaService
    {
        private readonly ISchemaRepository _schemaRepository;

        public SchemaService(ISchemaRepository schemaRepository)
        {
            _schemaRepository = schemaRepository;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<SchemaDto>> GetSchemasAsync(CancellationToken cancellationToken)
        {
            var schemas = await _schemaRepository.GetSchemasAsync(cancellationToken);
            return schemas.Select(x => new SchemaDto(x));
        }

        /// <inheritdoc />
        public async Task<SchemaDto> GetSchemaAsync(Guid schemaId, CancellationToken cancellationToken)
        {
            var schema = await _schemaRepository.GetSchemaAsync(schemaId, cancellationToken);
            return new SchemaDto(schema);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<SchemaVersionDto>> GetSchemaVersionsAsync(Guid schemaId, CancellationToken cancellationToken)
        {
            var versions = await _schemaRepository.GetSchemaVersionsAsync(schemaId, cancellationToken);
            return versions.Select(x => new SchemaVersionDto(x));
        }

        /// <inheritdoc />
        public async Task<SchemaDto> InsertSchemaAsync(SchemaDto schema, CancellationToken cancellationToken)
        {
            // Version is entirely system-managed, never client-supplied.
            schema.Version = "v1";
            var createdSchema = await _schemaRepository.InsertSchemaAsync(schema, cancellationToken);
            return new SchemaDto(createdSchema);
        }

        /// <inheritdoc />
        public async Task<SchemaDto> UpdateSchemaAsync(SchemaDto schema, CancellationToken cancellationToken)
        {
            var existingVersions = (await _schemaRepository.GetSchemaVersionsAsync(schema.Id, cancellationToken)).ToList();
            schema.Version = $"v{existingVersions.Count + 1}";

            var updatedSchema = await _schemaRepository.UpdateSchemaAsync(schema, cancellationToken);
            return new SchemaDto(updatedSchema);
        }

        /// <inheritdoc />
        public Task RemoveSchemaAsync(Guid schemaId, CancellationToken cancellationToken)
        {
            return _schemaRepository.RemoveSchemaAsync(schemaId, cancellationToken);
        }
    }
}
