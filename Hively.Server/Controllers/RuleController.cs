using Hively.Server.Dto;
using Hively.Server.Exceptions;
using Hively.Server.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Hively.Server.Controllers
{
    /// <summary>
    /// Controller for managing Rule entities.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class RuleController : ControllerBase
    {
        private readonly ILogger<RuleController> _logger;
        private readonly IRuleService _ruleService;

        public RuleController(ILogger<RuleController> logger, IRuleService ruleService)
        {
            _logger = logger;
            _ruleService = ruleService;
        }

        /// <summary>
        /// Retrieves all rules.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetRulesAsync()
        {
            try
            {
                var rules = (await _ruleService.GetRulesAsync(CancellationToken.None)).ToList();
                _logger.LogInformation("Retrieved {RuleCount} rules.", rules.Count);
                return Ok(rules);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting rules.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Retrieves a single rule by ID.
        /// </summary>
        [HttpGet("{ruleId}")]
        public async Task<IActionResult> GetRuleByIdAsync(Guid ruleId)
        {
            try
            {
                var rule = await _ruleService.GetRuleAsync(ruleId, CancellationToken.None);
                _logger.LogInformation("Retrieved rule {RuleId}.", ruleId);
                return Ok(rule);
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "Rule {RuleId} not found.", ruleId);
                return StatusCode(StatusCodes.Status404NotFound, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting rule {RuleId}.", ruleId);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Creates a new rule.
        /// </summary>
        [HttpPut("insert")]
        public async Task<IActionResult> InsertRuleAsync([FromBody] RuleDto rule)
        {
            try
            {
                var createdRule = await _ruleService.InsertRuleAsync(rule, CancellationToken.None);
                _logger.LogInformation("Created rule {RuleId} ({RulePattern}).", createdRule.Id, createdRule.Pattern);
                return Ok(createdRule);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while inserting rule {RulePattern}.", rule.Pattern);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Updates an existing rule.
        /// </summary>
        [HttpPost("update")]
        public async Task<IActionResult> UpdateRuleAsync([FromBody] RuleDto rule)
        {
            try
            {
                var updatedRule = await _ruleService.UpdateRuleAsync(rule, CancellationToken.None);
                _logger.LogInformation("Updated rule {RuleId}.", updatedRule.Id);
                return Ok(updatedRule);
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "Rule {RuleId} not found.", rule.Id);
                return StatusCode(StatusCodes.Status404NotFound, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating rule {RuleId}.", rule.Id);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Deletes a rule.
        /// </summary>
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteRuleAsync(Guid ruleId)
        {
            try
            {
                await _ruleService.RemoveRuleAsync(ruleId, CancellationToken.None);
                _logger.LogInformation("Removed rule {RuleId}.", ruleId);
                return Ok(new { message = "Removal successful" });
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "Rule {RuleId} not found.", ruleId);
                return StatusCode(StatusCodes.Status404NotFound, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while removing rule {RuleId}.", ruleId);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
