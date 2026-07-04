using System.Text.Json;
using Hively.Server.Dto;
using Hively.Server.Repository.Abstractions;
using Hively.Server.Services.Abstractions;

namespace Hively.Server.Services.Ingestion
{
    /// <summary>
    /// Business logic for recording an incoming MQTT message against its topic.
    /// </summary>
    public class TopicIngestionService : ITopicIngestionService
    {
        private readonly ITopicRepository _topicRepository;

        public TopicIngestionService(ITopicRepository topicRepository)
        {
            _topicRepository = topicRepository;
        }

        /// <inheritdoc />
        public async Task IngestMessageAsync(
            string topicPath,
            string rawPayload,
            bool retained,
            DateTime receivedAtUtc,
            CancellationToken cancellationToken)
        {
            var topic = await _topicRepository.FindTopicByPathAsync(topicPath, cancellationToken);
            if (topic == null)
            {
                var newTopicId = await _topicRepository.InsertTopicAsync(new TopicDto { Path = topicPath }, cancellationToken);
                topic = await _topicRepository.GetTopicAsync(newTopicId, cancellationToken);
            }

            var payloadJson = NormalizeToJsonPayload(rawPayload);

            var (compliant, _) = SchemaComplianceValidator.Validate(topic.Schema?.Definition, payloadJson);
            var violationCount = topic.ViolationCount + (compliant == false ? 1 : 0);

            var currentHistogram = JsonSerializer.Deserialize<int[]>(topic.ActivityHistogram);
            var newHistogram = ActivityHistogramRoller.Roll(currentHistogram, topic.LastSeenAt, receivedAtUtc);

            await _topicRepository.RecordIngestedMessageAsync(
                topic.Id,
                payloadJson,
                receivedAtUtc,
                retained,
                violationCount,
                JsonSerializer.Serialize(newHistogram),
                cancellationToken);
        }

        // MQTT payloads are arbitrary bytes, but last_payload is a jsonb column and
        // schema validation expects JSON — a non-JSON payload gets wrapped as a JSON
        // string scalar so the write never fails and validation reports a mismatch
        // instead of crashing on it.
        private static string NormalizeToJsonPayload(string rawPayload)
        {
            try
            {
                JsonDocument.Parse(rawPayload).Dispose();
                return rawPayload;
            }
            catch (JsonException)
            {
                return JsonSerializer.Serialize(rawPayload);
            }
        }
    }
}
