using Hively.Server.Dto;
using Hively.Server.Exceptions;
using Hively.Server.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hively.Server.Controllers
{
    /// <summary>
    /// Controller for managing Match entities.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MatchController : ControllerBase
    {
        private readonly ILogger<MatchController> _logger;
        private readonly IMatchService _matchService;

        public MatchController(ILogger<MatchController> logger, IMatchService matchService)
        {
            _logger = logger;
            _matchService = matchService;
        }

        /// <summary>
        /// Retrieves all admin-authored matches.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMatchesAsync()
        {
            try
            {
                var matches = (await _matchService.GetMatchesAsync(CancellationToken.None)).ToList();
                _logger.LogInformation("Retrieved {MatchCount} matches.", matches.Count);
                return Ok(matches);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting matches.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Retrieves a single match by ID.
        /// </summary>
        [HttpGet("{matchId}")]
        public async Task<IActionResult> GetMatchByIdAsync(Guid matchId)
        {
            try
            {
                var match = await _matchService.GetMatchAsync(matchId, CancellationToken.None);
                _logger.LogInformation("Retrieved match {MatchId}.", matchId);
                return Ok(match);
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "Match {MatchId} not found.", matchId);
                return StatusCode(StatusCodes.Status404NotFound, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting match {MatchId}.", matchId);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Creates a new match.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPut("insert")]
        public async Task<IActionResult> InsertMatchAsync([FromBody] MatchDto match)
        {
            try
            {
                var createdMatch = await _matchService.InsertMatchAsync(match, CancellationToken.None);
                _logger.LogInformation("Created match {MatchId} ({MatchPattern}).", createdMatch.Id, createdMatch.Pattern);
                return Ok(createdMatch);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while inserting match {MatchPattern}.", match.Pattern);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Updates an existing match.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("update")]
        public async Task<IActionResult> UpdateMatchAsync([FromBody] MatchDto match)
        {
            try
            {
                var updatedMatch = await _matchService.UpdateMatchAsync(match, CancellationToken.None);
                _logger.LogInformation("Updated match {MatchId}.", updatedMatch.Id);
                return Ok(updatedMatch);
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "Match {MatchId} not found.", match.Id);
                return StatusCode(StatusCodes.Status404NotFound, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating match {MatchId}.", match.Id);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Deletes a match.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteMatchAsync(Guid matchId)
        {
            try
            {
                await _matchService.RemoveMatchAsync(matchId, CancellationToken.None);
                _logger.LogInformation("Removed match {MatchId}.", matchId);
                return Ok(new { message = "Removal successful" });
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "Match {MatchId} not found.", matchId);
                return StatusCode(StatusCodes.Status404NotFound, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while removing match {MatchId}.", matchId);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
