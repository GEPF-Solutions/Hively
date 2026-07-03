using Hively.Server.Dto;
using Hively.Server.Exceptions;
using Hively.Server.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hively.Server.Controllers
{
    /// <summary>
    /// Controller for managing Producer entities.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProducerController : ControllerBase
    {
        private readonly ILogger<ProducerController> _logger;
        private readonly IProducerService _producerService;

        public ProducerController(ILogger<ProducerController> logger, IProducerService producerService)
        {
            _logger = logger;
            _producerService = producerService;
        }

        /// <summary>
        /// Retrieves all producers.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProducersAsync()
        {
            try
            {
                var producers = (await _producerService.GetProducersAsync(CancellationToken.None)).ToList();
                _logger.LogInformation("Retrieved {ProducerCount} producers.", producers.Count);
                return Ok(producers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting producers.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Retrieves a single producer by ID.
        /// </summary>
        [HttpGet("{producerId}")]
        public async Task<IActionResult> GetProducerByIdAsync(Guid producerId)
        {
            try
            {
                var producer = await _producerService.GetProducerAsync(producerId, CancellationToken.None);
                _logger.LogInformation("Retrieved producer {ProducerId}.", producerId);
                return Ok(producer);
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "Producer {ProducerId} not found.", producerId);
                return StatusCode(StatusCodes.Status404NotFound, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting producer {ProducerId}.", producerId);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Creates a new producer.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPut("insert")]
        public async Task<IActionResult> InsertProducerAsync([FromBody] ProducerDto producer)
        {
            try
            {
                var createdProducer = await _producerService.InsertProducerAsync(producer, CancellationToken.None);
                _logger.LogInformation("Created producer {ProducerId} ({ProducerName}).", createdProducer.Id, createdProducer.Name);
                return Ok(createdProducer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while inserting producer {ProducerName}.", producer.Name);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Updates an existing producer.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("update")]
        public async Task<IActionResult> UpdateProducerAsync([FromBody] ProducerDto producer)
        {
            try
            {
                await _producerService.UpdateProducerAsync(producer, CancellationToken.None);
                _logger.LogInformation("Updated producer {ProducerId}.", producer.Id);
                return Ok(new { message = "Update successful" });
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "Producer {ProducerId} not found.", producer.Id);
                return StatusCode(StatusCodes.Status404NotFound, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating producer {ProducerId}.", producer.Id);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Deletes a producer.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteProducerAsync(Guid producerId)
        {
            try
            {
                await _producerService.RemoveProducerAsync(producerId, CancellationToken.None);
                _logger.LogInformation("Removed producer {ProducerId}.", producerId);
                return Ok(new { message = "Removal successful" });
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "Producer {ProducerId} not found.", producerId);
                return StatusCode(StatusCodes.Status404NotFound, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while removing producer {ProducerId}.", producerId);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
