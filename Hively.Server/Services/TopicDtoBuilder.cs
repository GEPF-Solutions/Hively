using Hively.Server.DbModel;
using Hively.Server.Dto;

namespace Hively.Server.Services
{
    /// <summary>
    /// Builds a <see cref="TopicDto"/> from a <see cref="Topic"/> entity with
    /// compliance computed against its assigned schema (see
    /// <see cref="SchemaComplianceValidator"/>). Shared by <see cref="TopicService"/>
    /// and <see cref="Ingestion.TopicIngestionService"/> so both read paths and the
    /// ingestion push path agree on what compliance means.
    /// </summary>
    public static class TopicDtoBuilder
    {
        public static TopicDto Build(Topic topic)
        {
            var dto = new TopicDto(topic);
            (dto.Compliant, dto.Mismatches) = SchemaComplianceValidator.Validate(topic.Schema?.Definition, dto.LastPayload);
            return dto;
        }
    }
}
