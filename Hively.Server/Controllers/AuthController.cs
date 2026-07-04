using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hively.Server.Controllers
{
    /// <summary>
    /// Controller for the login/logout flow. Both external providers sign into the
    /// same cookie scheme (see Program.cs); app-level role is looked up from our own
    /// Users table via IUserService.UpsertFromExternalLoginAsync, called from each
    /// provider's post-authentication event (see Program.cs), not from here.
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;

        public AuthController(ILogger<AuthController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Which external providers are enabled (Authentication:{Provider}:Enabled in
        /// config) — anonymous, since the login page needs this before anyone's
        /// signed in, to know which buttons to show at all.
        /// </summary>
        [HttpGet("providers")]
        public IActionResult Providers()
        {
            return Ok(new
            {
                google = _configuration.GetValue<bool>("Authentication:Google:Enabled"),
                entra = _configuration.GetValue<bool>("Authentication:Entra:Enabled")
            });
        }

        /// <summary>
        /// Starts the Google sign-in challenge; redirects back to <paramref name="returnUrl"/> on success.
        /// </summary>
        [HttpGet("login/google")]
        public IActionResult LoginGoogle([FromQuery] string returnUrl = "/")
        {
            if (!_configuration.GetValue<bool>("Authentication:Google:Enabled"))
            {
                return NotFound("Google sign-in is not enabled.");
            }

            return Challenge(BuildProperties(returnUrl), GoogleDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// Starts the Microsoft Entra ID sign-in challenge; redirects back to <paramref name="returnUrl"/> on success.
        /// </summary>
        [HttpGet("login/entra")]
        public IActionResult LoginEntra([FromQuery] string returnUrl = "/")
        {
            if (!_configuration.GetValue<bool>("Authentication:Entra:Enabled"))
            {
                return NotFound("Entra sign-in is not enabled.");
            }

            return Challenge(BuildProperties(returnUrl), "Entra");
        }

        /// <summary>
        /// Signs out of the local cookie session.
        /// </summary>
        [HttpPost("logout")]
        public async Task<IActionResult> LogoutAsync()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _logger.LogInformation("User {Email} logged out.", User.FindFirstValue(ClaimTypes.Email));
            return Ok();
        }

        /// <summary>
        /// Returns the current signed-in user's email and app-level role.
        /// </summary>
        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            return Ok(new
            {
                email = User.FindFirstValue(ClaimTypes.Email),
                role = User.FindFirstValue(ClaimTypes.Role)
            });
        }

        // Only ever redirect back into this app — an unvalidated returnUrl from the
        // query string would otherwise let an attacker phish a valid session onto an
        // external site (open redirect).
        private AuthenticationProperties BuildProperties(string returnUrl)
        {
            var safeReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : "/";
            return new AuthenticationProperties { RedirectUri = safeReturnUrl };
        }
    }
}
