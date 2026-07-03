using Hively.Server.Dto;
using Hively.Server.Exceptions;
using Hively.Server.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hively.Server.Controllers
{
    /// <summary>
    /// Controller for managing app-level user accounts and roles. Admin-only —
    /// there is no self-service admin signup; an existing Admin promotes others.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IUserService _userService;

        public UserController(ILogger<UserController> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        /// <summary>
        /// Retrieves all users.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUsersAsync()
        {
            try
            {
                var users = (await _userService.GetUsersAsync(CancellationToken.None)).ToList();
                _logger.LogInformation("Retrieved {UserCount} users.", users.Count);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting users.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Updates a user's role (promote/demote between Admin and Viewer).
        /// </summary>
        [HttpPost("{userId}/role")]
        public async Task<IActionResult> UpdateRoleAsync(Guid userId, [FromBody] UpdateRoleDto body)
        {
            try
            {
                var updatedUser = await _userService.UpdateRoleAsync(userId, body.Role, CancellationToken.None);
                _logger.LogInformation("Updated role for user {UserId} to {Role}.", userId, body.Role);
                return Ok(updatedUser);
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "User {UserId} not found.", userId);
                return StatusCode(StatusCodes.Status404NotFound, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid role update for user {UserId}.", userId);
                return StatusCode(StatusCodes.Status400BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating role for user {UserId}.", userId);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
