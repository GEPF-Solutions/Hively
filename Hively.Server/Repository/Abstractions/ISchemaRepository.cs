using Hively.Server.DbModel;
using Hively.Server.Dto;

namespace Hively.Server.Repository.Abstractions
{
    /// <summary>
    /// Repository interface for managing Schema entities and their version history.
    /// </summary>
    public interface ISchemaRepository
    {
        /// <summary>
        /// Retrieves all schemas (current version only, not history).
        /// </summary>
        Task<IEnumerable<Schema>> GetSchemasAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves a single schema by ID.
        /// </summary>
        /// <exception cref="EntityNotFoundException">Thrown when schema is not found.</exception>
        Task<Schema> GetSchemaAsync(Guid schemaId, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves the version history for a schema, most recent first.
        /// </summary>
        Task<IEnumerable<SchemaVersion>> GetSchemaVersionsAsync(Guid schemaId, CancellationToken cancellationToken);

        /// <summary>
        /// Creates a new schema and its first version history entry.
        /// </summary>
        Task<Schema> InsertSchemaAsync(SchemaDto schema, CancellationToken cancellationToken);

        /// <summary>
        /// Updates a schema's definition and appends a new version history entry.
        /// The caller (service) is responsible for computing the next version string.
        /// </summary>
        /// <exception cref="EntityNotFoundException">Thrown when schema is not found.</exception>
        Task<Schema> UpdateSchemaAsync(SchemaDto schema, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes a schema and its version history (ON DELETE CASCADE). Topics
        /// referencing it have their schema reference set to null — no in-use
        /// check here, deletion is never blocked.
        /// </summary>
        /// <exception cref="EntityNotFoundException">Thrown when schema is not found.</exception>
        Task RemoveSchemaAsync(Guid schemaId, CancellationToken cancellationToken);
    }
}
