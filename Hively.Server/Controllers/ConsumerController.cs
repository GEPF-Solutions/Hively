using Hively.Server.Dto;
using Hively.Server.Exceptions;
using Hively.Server.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Hively.Server.Controllers
{
    /// <summary>
    /// Controller for managing Consumer entities.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ConsumerController : ControllerBase
    {
        private readonly ILogger<ConsumerController> _logger;
        private readonly IConsumerService _consumerService;

        public ConsumerController(ILogger<ConsumerController> logger, IConsumerService consumerService)
        {
            _logger = logger;
            _consumerService = consumerService;
        }

        /// <summary>
        /// Retrieves all consumers.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetConsumersAsync()
        {
            try
            {
                var consumers = (await _consumerService.GetConsumersAsync(CancellationToken.None)).ToList();
                _logger.LogInformation("Retrieved {ConsumerCount} consumers.", consumers.Count);
                return Ok(consumers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting consumers.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Retrieves a single consumer by ID.
        /// </summary>
        [HttpGet("{consumerId}")]
        public async Task<IActionResult> GetConsumerByIdAsync(Guid consumerId)
        {
            try
            {
                var consumer = await _consumerService.GetConsumerAsync(consumerId, CancellationToken.None);
                _logger.LogInformation("Retrieved consumer {ConsumerId}.", consumerId);
                return Ok(consumer);
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "Consumer {ConsumerId} not found.", consumerId);
                return StatusCode(StatusCodes.Status404NotFound, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting consumer {ConsumerId}.", consumerId);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Creates a new consumer.
        /// </summary>
        [HttpPut("insert")]
        public async Task<IActionResult> InsertConsumerAsync([FromBody] ConsumerDto consumer)
        {
            try
            {
                var createdConsumer = await _consumerService.InsertConsumerAsync(consumer, CancellationToken.None);
                _logger.LogInformation("Created consumer {ConsumerId} ({ConsumerName}).", createdConsumer.Id, createdConsumer.Name);
                return Ok(createdConsumer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while inserting consumer {ConsumerName}.", consumer.Name);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Updates an existing consumer.
        /// </summary>
        [HttpPost("update")]
        public async Task<IActionResult> UpdateConsumerAsync([FromBody] ConsumerDto consumer)
        {
            try
            {
                await _consumerService.UpdateConsumerAsync(consumer, CancellationToken.None);
                _logger.LogInformation("Updated consumer {ConsumerId}.", consumer.Id);
                return Ok(new { message = "Update successful" });
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "Consumer {ConsumerId} not found.", consumer.Id);
                return StatusCode(StatusCodes.Status404NotFound, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating consumer {ConsumerId}.", consumer.Id);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Deletes a consumer.
        /// </summary>
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteConsumerAsync(Guid consumerId)
        {
            try
            {
                await _consumerService.RemoveConsumerAsync(consumerId, CancellationToken.None);
                _logger.LogInformation("Removed consumer {ConsumerId}.", consumerId);
                return Ok(new { message = "Removal successful" });
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogWarning(ex, "Consumer {ConsumerId} not found.", consumerId);
                return StatusCode(StatusCodes.Status404NotFound, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while removing consumer {ConsumerId}.", consumerId);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
