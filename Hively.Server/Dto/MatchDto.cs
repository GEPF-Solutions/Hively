using Hively.Server.DbModel;

namespace Hively.Server.Dto
{
    public class MatchDto
    {
        public Guid Id { get; set; }

        /// <summary>Optional label — falls back to showing the pattern when not set.</summary>
        public string? Name { get; set; }

        /// <summary>MQTT-style topic filter using '+' and '#' wildcards.</summary>
        public string Pattern { get; set; } = null!;

        /// <summary>
        /// Set only for the single private per-topic override a manual TopicDetail
        /// edit creates (its Pattern is that topic's own literal path). Informational —
        /// never admin-settable via the Manage Matches UI.
        /// </summary>
        public Guid? TopicId { get; set; }

        /// <summary>Set producer to this id. Mutually exclusive with <see cref="ExcludeProducer"/>.</summary>
        public Guid? ProducerId { get; set; }

        /// <summary>Explicitly clear producer, overriding any less-specific match that sets one.</summary>
        public bool ExcludeProducer { get; set; }

        /// <summary>Set schema to this id. Mutually exclusive with <see cref="ExcludeSchema"/>.</summary>
        public Guid? SchemaId { get; set; }

        /// <summary>Explicitly clear schema, overriding any less-specific match that sets one.</summary>
        public bool ExcludeSchema { get; set; }

        public List<MatchTagActionDto> TagActions { get; set; } = new();

        public List<MatchConsumerActionDto> ConsumerActions { get; set; } = new();

        /// <summary>
        /// When a newly-untracked topic matches this pattern (among possibly others),
        /// track it and resolve it immediately instead of waiting for an admin.
        /// </summary>
        public bool AutoApply { get; set; }

        /// <summary>Tie-breaker when two matches have equal specificity for the same field/value.</summary>
        public DateTime UpdatedAt { get; set; }

        public MatchDto(Match match)
        {
            Id = match.Id;
            Name = match.Name;
            Pattern = match.Pattern;
            TopicId = match.TopicId;
            ProducerId = match.ProducerId;
            ExcludeProducer = match.ExcludeProducer;
            SchemaId = match.SchemaId;
            ExcludeSchema = match.ExcludeSchema;
            TagActions = match.MatchTagActions
                .Select(a => new MatchTagActionDto { TagId = a.TagId, IsExclude = a.IsExclude })
                .ToList();
            ConsumerActions = match.MatchConsumerActions
                .Select(a => new MatchConsumerActionDto { ConsumerId = a.ConsumerId, IsExclude = a.IsExclude })
                .ToList();
            AutoApply = match.AutoApply;
            UpdatedAt = match.UpdatedAt;
        }

        // Empty constructor for deserialization
        public MatchDto()
        {
        }
    }

    public class MatchTagActionDto
    {
        public string TagId { get; set; } = null!;

        public bool IsExclude { get; set; }
    }

    public class MatchConsumerActionDto
    {
        public Guid ConsumerId { get; set; }

        public bool IsExclude { get; set; }
    }
}
