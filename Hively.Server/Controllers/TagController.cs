using Hively.Server.Dto;
using Hively.Server.Exceptions;
using Hively.Server.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hively.Server.Controllers
{
    /// <summary>
    /// Controller for managing Tag entities.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TagController : ControllerBase
    {
        private readonly ILogger<TagController> _logger;
        private readonly ITagService _tagService;

        public TagController(ILogger<TagController> logger, ITagService tagService)
        {
            _logger = logger;
            _tagService = tagService;
        }

        /// <summary>
        /// Retrieves all tags.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTagsAsync()
        {
            try
            {
                var tags = (await _tagService.GetTagsAsync(CancellationToken.None)).ToList();
                _logger.LogInformation("Retrieved {TagCount} tags.", tags.Count);
                return Ok(tags);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting tags.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Retrieves a single tag by ID (slug).
        /// </summary>
        [HttpGet("{tagId}")]
        public async Task<IActionResult> GetTagByIdAsync(string tagId)
        {
            try
            {
                var tag = await _tagService.GetTagAsync(tagId, CancellationToken.None);
                _logger.LogInformation("Retrieved tag {TagId}.", tagId);
                return Ok(tag);
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "Tag {TagId} not found.", tagId);
                return StatusCode(StatusCodes.Status404NotFound, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting tag {TagId}.", tagId);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Creates a new tag.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPut("insert")]
        public async Task<IActionResult> InsertTagAsync([FromBody] TagDto tag)
        {
            try
            {
                var createdTag = await _tagService.InsertTagAsync(tag, CancellationToken.None);
                _logger.LogInformation("Created tag {TagId} ({TagLabel}).", createdTag.Id, createdTag.Label);
                return Ok(createdTag);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while inserting tag {TagId}.", tag.Id);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Updates an existing tag.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("update")]
        public async Task<IActionResult> UpdateTagAsync([FromBody] TagDto tag)
        {
            try
            {
                await _tagService.UpdateTagAsync(tag, CancellationToken.None);
                _logger.LogInformation("Updated tag {TagId}.", tag.Id);
                return Ok(new { message = "Update successful" });
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "Tag {TagId} not found.", tag.Id);
                return StatusCode(StatusCodes.Status404NotFound, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating tag {TagId}.", tag.Id);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Deletes a tag.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteTagAsync(string tagId)
        {
            try
            {
                await _tagService.RemoveTagAsync(tagId, CancellationToken.None);
                _logger.LogInformation("Removed tag {TagId}.", tagId);
                return Ok(new { message = "Removal successful" });
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "Tag {TagId} not found.", tagId);
                return StatusCode(StatusCodes.Status404NotFound, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while removing tag {TagId}.", tagId);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
