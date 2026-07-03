using Hively.Server.Dto;

namespace Hively.Server.Services.Abstractions
{
    /// <summary>
    /// Service interface for Schema business logic, including version-history management.
    /// </summary>
    public interface ISchemaService
    {
        /// <summary>
        /// Retrieves all schemas.
        /// </summary>
        Task<IEnumerable<SchemaDto>> GetSchemasAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves a single schema by ID.
        /// </summary>
        Task<SchemaDto> GetSchemaAsync(Guid schemaId, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves the version history for a schema, most recent first.
        /// </summary>
        Task<IEnumerable<SchemaVersionDto>> GetSchemaVersionsAsync(Guid schemaId, CancellationToken cancellationToken);

        /// <summary>
        /// Creates a new schema, always starting at version "v1".
        /// </summary>
        Task<SchemaDto> InsertSchemaAsync(SchemaDto schema, CancellationToken cancellationToken);

        /// <summary>
        /// Updates a schema's definition, auto-incrementing its version and
        /// recording a new history entry — never overwrites in place.
        /// </summary>
        Task<SchemaDto> UpdateSchemaAsync(SchemaDto schema, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes a schema.
        /// </summary>
        Task RemoveSchemaAsync(Guid schemaId, CancellationToken cancellationToken);
    }
}
