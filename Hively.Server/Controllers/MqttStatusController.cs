using Hively.Server.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hively.Server.Controllers
{
    /// <summary>
    /// Read-only controller for the MQTT broker's current connection state
    /// (the header's connection indicator). Live changes push over
    /// <see cref="Hubs.TopicHub"/>; this is only for the initial fetch.
    /// </summary>
    [ApiController]
    [Route("api/mqtt-status")]
    [Authorize]
    public class MqttStatusController : ControllerBase
    {
        private readonly ILogger<MqttStatusController> _logger;
        private readonly IMqttStatusService _statusService;

        public MqttStatusController(ILogger<MqttStatusController> logger, IMqttStatusService statusService)
        {
            _logger = logger;
            _statusService = statusService;
        }

        /// <summary>
        /// Retrieves the current MQTT broker connection state.
        /// </summary>
        [HttpGet]
        public IActionResult GetStatus()
        {
            try
            {
                var status = _statusService.GetStatus();
                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting MQTT broker status.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
