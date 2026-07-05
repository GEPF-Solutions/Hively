using System.Security.Claims;
using Hively.Server.Dto;
using Hively.Server.Services.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hively.Server.Controllers
{
    /// <summary>
    /// Controller for the login/logout flow. External providers (Google, Entra) sign
    /// into the same cookie scheme as basic auth (see Program.cs); app-level role is
    /// looked up from our own Users table via IUserService.UpsertFromExternalLoginAsync,
    /// called from each provider's post-authentication event for Google/Entra, and
    /// directly from LoginBasic below since basic auth has no handler middleware.
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;

        public AuthController(ILogger<AuthController> logger, IConfiguration configuration, IUserService userService)
        {
            _logger = logger;
            _configuration = configuration;
            _userService = userService;
        }

        /// <summary>
        /// Which providers are enabled (Authentication:{Provider}:Enabled in config) and,
        /// for Google/Entra, whether their ClientId/ClientSecret are actually present —
        /// anonymous, since the login page needs this before anyone's signed in, to know
        /// which buttons/form to show, and whether an enabled OAuth button should render
        /// disabled because its secrets haven't been configured yet (e.g. no user-secrets
        /// set up on this machine). Basic has no such distinction: it never depends on
        /// external secrets, only its own Enabled flag.
        /// </summary>
        [HttpGet("providers")]
        public IActionResult Providers()
        {
            var googleEnabled = _configuration.GetValue<bool>("Authentication:Google:Enabled");
            var entraEnabled = _configuration.GetValue<bool>("Authentication:Entra:Enabled");
            return Ok(new
            {
                google = new { enabled = googleEnabled, configured = googleEnabled && IsGoogleConfigured() },
                entra = new { enabled = entraEnabled, configured = entraEnabled && IsEntraConfigured() },
                basic = _configuration.GetValue<bool>("Authentication:Basic:Enabled")
            });
        }

        /// <summary>
        /// Signs in with the single username/password configured via
        /// Authentication:Basic:Username/Password — for self-hosted setups with no
        /// Entra tenant or Google Workspace domain available. Compared in cleartext
        /// for now, not hashed.
        /// </summary>
        [HttpPost("login/basic")]
        public async Task<IActionResult> LoginBasic([FromBody] BasicLoginRequestDto request, CancellationToken cancellationToken)
        {
            if (!_configuration.GetValue<bool>("Authentication:Basic:Enabled"))
            {
                return NotFound("Basic sign-in is not enabled.");
            }

            var configuredUsername = _configuration["Authentication:Basic:Username"];
            var configuredPassword = _configuration["Authentication:Basic:Password"];
            if (request.Username != configuredUsername || request.Password != configuredPassword)
            {
                _logger.LogWarning("Failed basic-auth login attempt for username {Username}.", request.Username);
                return Unauthorized("Invalid username or password.");
            }

            var user = await _userService.UpsertFromExternalLoginAsync("basic", request.Username, request.Username, cancellationToken);

            var identity = new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.Email, request.Username),
                    new Claim(ClaimTypes.NameIdentifier, request.Username),
                    new Claim(ClaimTypes.Role, user.Role)
                ],
                CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
            _logger.LogInformation("User {Username} logged in via basic auth.", request.Username);

            return Ok(new { email = request.Username, role = user.Role });
        }

        /// <summary>
        /// Starts the Google sign-in challenge; redirects back to <paramref name="returnUrl"/> on success.
        /// </summary>
        [HttpGet("login/google")]
        public IActionResult LoginGoogle([FromQuery] string returnUrl = "/")
        {
            if (!_configuration.GetValue<bool>("Authentication:Google:Enabled") || !IsGoogleConfigured())
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
            if (!_configuration.GetValue<bool>("Authentication:Entra:Enabled") || !IsEntraConfigured())
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

        // Mirrors the same check Program.cs uses to decide whether to register the
        // Google handler at all — kept in sync manually since both need it and neither
        // depends on the other (Program.cs runs before DI exists to inject a shared check).
        private bool IsGoogleConfigured() =>
            !string.IsNullOrEmpty(_configuration["Authentication:Google:ClientId"])
            && !string.IsNullOrEmpty(_configuration["Authentication:Google:ClientSecret"]);

        private bool IsEntraConfigured() =>
            !string.IsNullOrEmpty(_configuration["Authentication:Entra:ClientId"])
            && !string.IsNullOrEmpty(_configuration["Authentication:Entra:ClientSecret"])
            && !string.IsNullOrEmpty(_configuration["Authentication:Entra:TenantId"]);
    }
}
