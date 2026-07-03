using Hively.Server.Dto;
using Hively.Server.Exceptions;
using Hively.Server.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hively.Server.Controllers
{
    /// <summary>
    /// Controller for managing Schema entities and their version history.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SchemaController : ControllerBase
    {
        private readonly ILogger<SchemaController> _logger;
        private readonly ISchemaService _schemaService;

        public SchemaController(ILogger<SchemaController> logger, ISchemaService schemaService)
        {
            _logger = logger;
            _schemaService = schemaService;
        }

        /// <summary>
        /// Retrieves all schemas.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetSchemasAsync()
        {
            try
            {
                var schemas = (await _schemaService.GetSchemasAsync(CancellationToken.None)).ToList();
                _logger.LogInformation("Retrieved {SchemaCount} schemas.", schemas.Count);
                return Ok(schemas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting schemas.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Retrieves a single schema by ID.
        /// </summary>
        [HttpGet("{schemaId}")]
        public async Task<IActionResult> GetSchemaByIdAsync(Guid schemaId)
        {
            try
            {
                var schema = await _schemaService.GetSchemaAsync(schemaId, CancellationToken.None);
                _logger.LogInformation("Retrieved schema {SchemaId}.", schemaId);
                return Ok(schema);
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "Schema {SchemaId} not found.", schemaId);
                return StatusCode(StatusCodes.Status404NotFound, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting schema {SchemaId}.", schemaId);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Retrieves the version history for a schema, most recent first.
        /// </summary>
        [HttpGet("{schemaId}/versions")]
        public async Task<IActionResult> GetSchemaVersionsAsync(Guid schemaId)
        {
            try
            {
                var versions = (await _schemaService.GetSchemaVersionsAsync(schemaId, CancellationToken.None)).ToList();
                _logger.LogInformation("Retrieved {VersionCount} versions for schema {SchemaId}.", versions.Count, schemaId);
                return Ok(versions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting versions for schema {SchemaId}.", schemaId);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Creates a new schema (always starts at version "v1").
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPut("insert")]
        public async Task<IActionResult> InsertSchemaAsync([FromBody] SchemaDto schema)
        {
            try
            {
                var createdSchema = await _schemaService.InsertSchemaAsync(schema, CancellationToken.None);
                _logger.LogInformation("Created schema {SchemaId} ({SchemaName}).", createdSchema.Id, createdSchema.Name);
                return Ok(createdSchema);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while inserting schema {SchemaName}.", schema.Name);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Updates a schema's definition, auto-incrementing its version.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("update")]
        public async Task<IActionResult> UpdateSchemaAsync([FromBody] SchemaDto schema)
        {
            try
            {
                var updatedSchema = await _schemaService.UpdateSchemaAsync(schema, CancellationToken.None);
                _logger.LogInformation("Updated schema {SchemaId} to {SchemaVersion}.", updatedSchema.Id, updatedSchema.Version);
                return Ok(updatedSchema);
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "Schema {SchemaId} not found.", schema.Id);
                return StatusCode(StatusCodes.Status404NotFound, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating schema {SchemaId}.", schema.Id);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Deletes a schema.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteSchemaAsync(Guid schemaId)
        {
            try
            {
                await _schemaService.RemoveSchemaAsync(schemaId, CancellationToken.None);
                _logger.LogInformation("Removed schema {SchemaId}.", schemaId);
                return Ok(new { message = "Removal successful" });
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "Schema {SchemaId} not found.", schemaId);
                return StatusCode(StatusCodes.Status404NotFound, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while removing schema {SchemaId}.", schemaId);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
