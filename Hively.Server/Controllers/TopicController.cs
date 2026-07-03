using Hively.Server.Dto;
using Hively.Server.Exceptions;
using Hively.Server.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Hively.Server.Controllers
{
    /// <summary>
    /// Controller for managing Topic entities. Scoped to CRUD and relationship
    /// assignment — MQTT ingestion, the relocation-relink heuristic, and rule
    /// matching are separate, not-yet-built subsystems.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TopicController : ControllerBase
    {
        private readonly ILogger<TopicController> _logger;
        private readonly ITopicService _topicService;

        public TopicController(ILogger<TopicController> logger, ITopicService topicService)
        {
            _logger = logger;
            _topicService = topicService;
        }

        /// <summary>
        /// Retrieves all topics.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTopicsAsync()
        {
            try
            {
                var topics = (await _topicService.GetTopicsAsync(CancellationToken.None)).ToList();
                _logger.LogInformation("Retrieved {TopicCount} topics.", topics.Count);
                return Ok(topics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting topics.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Retrieves a single topic by ID.
        /// </summary>
        [HttpGet("{topicId}")]
        public async Task<IActionResult> GetTopicByIdAsync(Guid topicId)
        {
            try
            {
                var topic = await _topicService.GetTopicAsync(topicId, CancellationToken.None);
                _logger.LogInformation("Retrieved topic {TopicId}.", topicId);
                return Ok(topic);
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "Topic {TopicId} not found.", topicId);
                return StatusCode(StatusCodes.Status404NotFound, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting topic {TopicId}.", topicId);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Creates a new topic stub (path only — mirrors what MQTT ingestion will do later).
        /// </summary>
        [HttpPut("insert")]
        public async Task<IActionResult> InsertTopicAsync([FromBody] TopicDto topic)
        {
            try
            {
                var createdTopic = await _topicService.InsertTopicAsync(topic, CancellationToken.None);
                _logger.LogInformation("Created topic {TopicId} ({TopicPath}).", createdTopic.Id, createdTopic.Path);
                return Ok(createdTopic);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while inserting topic {TopicPath}.", topic.Path);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Updates a topic's tracked/producer/schema/consumer/tag assignment ("Configure Topic").
        /// </summary>
        [HttpPost("update")]
        public async Task<IActionResult> UpdateTopicAsync([FromBody] TopicConfigureDto topic)
        {
            try
            {
                var updatedTopic = await _topicService.UpdateTopicAsync(topic, CancellationToken.None);
                _logger.LogInformation("Updated topic {TopicId}.", updatedTopic.Id);
                return Ok(updatedTopic);
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "Topic {TopicId} not found.", topic.Id);
                return StatusCode(StatusCodes.Status404NotFound, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating topic {TopicId}.", topic.Id);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Clears the violation counter for a topic.
        /// </summary>
        [HttpPost("{topicId}/clear-violations")]
        public async Task<IActionResult> ClearViolationsAsync(Guid topicId)
        {
            try
            {
                var updatedTopic = await _topicService.ClearViolationsAsync(topicId, CancellationToken.None);
                _logger.LogInformation("Cleared violations for topic {TopicId}.", topicId);
                return Ok(updatedTopic);
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "Topic {TopicId} not found.", topicId);
                return StatusCode(StatusCodes.Status404NotFound, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while clearing violations for topic {TopicId}.", topicId);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Deletes a topic.
        /// </summary>
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteTopicAsync(Guid topicId)
        {
            try
            {
                await _topicService.RemoveTopicAsync(topicId, CancellationToken.None);
                _logger.LogInformation("Removed topic {TopicId}.", topicId);
                return Ok(new { message = "Removal successful" });
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "Topic {TopicId} not found.", topicId);
                return StatusCode(StatusCodes.Status404NotFound, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while removing topic {TopicId}.", topicId);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
