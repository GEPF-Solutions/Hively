using Hively.Server.DbModel;
using Hively.Server.Dto;
using Hively.Server.Repository.Abstractions;
using Hively.Server.Services.Abstractions;

namespace Hively.Server.Services
{
    /// <summary>
    /// Service for managing Topic entities, including compliance computation.
    /// </summary>
    public class TopicService : ITopicService
    {
        private readonly ITopicRepository _topicRepository;

        public TopicService(ITopicRepository topicRepository)
        {
            _topicRepository = topicRepository;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TopicDto>> GetTopicsAsync(CancellationToken cancellationToken)
        {
            var topics = await _topicRepository.GetTopicsAsync(cancellationToken);
            return topics.Select(BuildDto);
        }

        /// <inheritdoc />
        public async Task<TopicDto> GetTopicAsync(Guid topicId, CancellationToken cancellationToken)
        {
            var topic = await _topicRepository.GetTopicAsync(topicId, cancellationToken);
            return BuildDto(topic);
        }

        /// <inheritdoc />
        public async Task<TopicDto> InsertTopicAsync(TopicDto topic, CancellationToken cancellationToken)
        {
            var createdTopicId = await _topicRepository.InsertTopicAsync(topic, cancellationToken);
            var createdTopic = await _topicRepository.GetTopicAsync(createdTopicId, cancellationToken);
            return BuildDto(createdTopic);
        }

        /// <inheritdoc />
        public async Task<TopicDto> UpdateTopicAsync(TopicConfigureDto topic, CancellationToken cancellationToken)
        {
            await _topicRepository.UpdateTopicAsync(topic, cancellationToken);
            var updatedTopic = await _topicRepository.GetTopicAsync(topic.Id, cancellationToken);
            return BuildDto(updatedTopic);
        }

        /// <inheritdoc />
        public async Task<TopicDto> ClearViolationsAsync(Guid topicId, CancellationToken cancellationToken)
        {
            await _topicRepository.ClearViolationsAsync(topicId, cancellationToken);
            var updatedTopic = await _topicRepository.GetTopicAsync(topicId, cancellationToken);
            return BuildDto(updatedTopic);
        }

        /// <inheritdoc />
        public Task RemoveTopicAsync(Guid topicId, CancellationToken cancellationToken)
        {
            return _topicRepository.RemoveTopicAsync(topicId, cancellationToken);
        }

        private static TopicDto BuildDto(Topic topic)
        {
            var dto = new TopicDto(topic);
            (dto.Compliant, dto.Mismatches) = SchemaComplianceValidator.Validate(topic.Schema?.Definition, dto.LastPayload);
            return dto;
        }
    }
}
