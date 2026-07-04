using Hively.Server.Dto;
using Hively.Server.Exceptions;
using Hively.Server.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hively.Server.Controllers
{
    /// <summary>
    /// Controller for managing Topic entities: CRUD, relationship assignment,
    /// rule matching/application, and relocation-relink suggestions. MQTT
    /// ingestion is a separate, not-yet-built subsystem.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
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
        [Authorize(Roles = "Admin")]
        [HttpPut("insert")]
        public async Task<IActionResult> InsertTopicAsync([FromBody] TopicDto topic)
        {
            try
            {
                var createdTopic = await _topicService.InsertTopicAsync(topic, CancellationToken.None);
                _logger.LogInformation("Created topic {TopicId} ({TopicPath}).", createdTopic.Id, createdTopic.Path);
                return Ok(createdTopic);
            }
            catch (DuplicateEntityException ex)
            {
                _logger.LogWarning(ex, "Duplicate topic path {TopicPath}.", topic.Path);
                return StatusCode(StatusCodes.Status409Conflict, ex.Message);
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
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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

        /// <summary>
        /// Finds rules matching this topic's path, ranked by specificity (most specific first, marked recommended).
        /// </summary>
        [HttpGet("{topicId}/matching-rules")]
        public async Task<IActionResult> GetMatchingRulesAsync(Guid topicId)
        {
            try
            {
                var matches = (await _topicService.FindMatchingRulesAsync(topicId, CancellationToken.None)).ToList();
                _logger.LogInformation("Found {MatchCount} matching rules for topic {TopicId}.", matches.Count, topicId);
                return Ok(matches);
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "Topic {TopicId} not found.", topicId);
                return StatusCode(StatusCodes.Status404NotFound, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while finding matching rules for topic {TopicId}.", topicId);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Applies a rule's producer/tag assignment to a topic and marks it tracked ("⚡ Apply rule").
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("{topicId}/apply-rule/{ruleId}")]
        public async Task<IActionResult> ApplyRuleAsync(Guid topicId, Guid ruleId)
        {
            try
            {
                var updatedTopic = await _topicService.ApplyRuleAsync(topicId, ruleId, CancellationToken.None);
                _logger.LogInformation("Applied rule {RuleId} to topic {TopicId}.", ruleId, topicId);
                return Ok(updatedTopic);
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "Topic {TopicId} or rule {RuleId} not found.", topicId, ruleId);
                return StatusCode(StatusCodes.Status404NotFound, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while applying rule {RuleId} to topic {TopicId}.", ruleId, topicId);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Looks for a stale, tracked topic that looks like this untracked topic's "old address" after a relocation.
        /// </summary>
        [HttpGet("{topicId}/relink-candidate")]
        public async Task<IActionResult> GetRelinkCandidateAsync(Guid topicId)
        {
            try
            {
                var candidate = await _topicService.FindRelinkCandidateAsync(topicId, CancellationToken.None);
                _logger.LogInformation("Relink candidate lookup for topic {TopicId}: {Found}.", topicId, candidate != null);
                return Ok(candidate);
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "Topic {TopicId} not found.", topicId);
                return StatusCode(StatusCodes.Status404NotFound, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while finding a relink candidate for topic {TopicId}.", topicId);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Accepts a relink suggestion: inherits producer/schema/tags/violation history from the old
        /// topic onto this one and retires the old topic record.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("{topicId}/accept-relink/{oldTopicId}")]
        public async Task<IActionResult> AcceptRelinkAsync(Guid topicId, Guid oldTopicId)
        {
            try
            {
                var updatedTopic = await _topicService.AcceptRelinkAsync(topicId, oldTopicId, CancellationToken.None);
                _logger.LogInformation("Relinked topic {TopicId} from retired topic {OldTopicId}.", topicId, oldTopicId);
                return Ok(updatedTopic);
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "Topic {TopicId} or {OldTopicId} not found.", topicId, oldTopicId);
                return StatusCode(StatusCodes.Status404NotFound, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while accepting relink for topic {TopicId}.", topicId);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
