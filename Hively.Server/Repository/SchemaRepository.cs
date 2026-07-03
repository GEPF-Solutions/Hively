using Hively.Server.DbModel;
using Hively.Server.Dto;
using Hively.Server.Exceptions;
using Hively.Server.Repository.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Hively.Server.Repository
{
    /// <summary>
    /// Repository for managing Schema entities and their version history in the database.
    /// </summary>
    public class SchemaRepository : ISchemaRepository
    {
        private readonly HivelyContext _dbContext;

        public SchemaRepository(HivelyContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Schema>> GetSchemasAsync(CancellationToken cancellationToken)
        {
            return await _dbContext.Schemas.AsNoTracking()
                .OrderBy(x => x.Name)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Schema> GetSchemaAsync(Guid schemaId, CancellationToken cancellationToken)
        {
            var schema = await _dbContext.Schemas.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == schemaId, cancellationToken);

            if (schema == null)
            {
                throw new EntityNotFoundException($"Schema id {schemaId} did not reference a valid schema.");
            }

            return schema;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<SchemaVersion>> GetSchemaVersionsAsync(Guid schemaId, CancellationToken cancellationToken)
        {
            return await _dbContext.SchemaVersions.AsNoTracking()
                .Where(x => x.SchemaId == schemaId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Schema> InsertSchemaAsync(SchemaDto schemaDto, CancellationToken cancellationToken)
        {
            var newSchema = new Schema
            {
                Name = schemaDto.Name,
                Definition = schemaDto.Definition,
                Version = schemaDto.Version! // always set by the service before this is called
            };

            await _dbContext.Schemas.AddAsync(newSchema, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _dbContext.SchemaVersions.AddAsync(new SchemaVersion
            {
                SchemaId = newSchema.Id,
                Version = newSchema.Version,
                Definition = newSchema.Definition,
                CreatedAt = DateTime.UtcNow
            }, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return newSchema;
        }

        /// <inheritdoc />
        public async Task<Schema> UpdateSchemaAsync(SchemaDto schemaDto, CancellationToken cancellationToken)
        {
            var schemaToUpdate = await _dbContext.Schemas
                .FirstOrDefaultAsync(x => x.Id == schemaDto.Id, cancellationToken);

            if (schemaToUpdate == null)
            {
                throw new EntityNotFoundException($"Schema id {schemaDto.Id} did not reference a valid schema.");
            }

            schemaToUpdate.Name = schemaDto.Name;
            schemaToUpdate.Definition = schemaDto.Definition;
            schemaToUpdate.Version = schemaDto.Version!; // always set by the service before this is called

            await _dbContext.SchemaVersions.AddAsync(new SchemaVersion
            {
                SchemaId = schemaToUpdate.Id,
                Version = schemaDto.Version!,
                Definition = schemaDto.Definition,
                CreatedAt = DateTime.UtcNow
            }, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return schemaToUpdate;
        }

        /// <inheritdoc />
        public async Task RemoveSchemaAsync(Guid schemaId, CancellationToken cancellationToken)
        {
            var schemaToRemove = await _dbContext.Schemas
                .FirstOrDefaultAsync(x => x.Id == schemaId, cancellationToken);

            if (schemaToRemove == null)
            {
                throw new EntityNotFoundException($"Schema id {schemaId} did not reference a valid schema.");
            }

            _dbContext.Schemas.Remove(schemaToRemove);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
