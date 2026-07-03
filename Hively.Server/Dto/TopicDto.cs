using Hively.Server.DbModel;

namespace Hively.Server.Dto
{
    public class TopicDto
    {
        public Guid Id { get; set; }
        public string Path { get; set; } = null!;
        public bool Tracked { get; set; }
        public Guid? ProducerId { get; set; }
        public Guid? SchemaId { get; set; }
        public string? LastPayload { get; set; }
        public DateTime? LastSeenAt { get; set; }
        public bool Retained { get; set; }
        public int ViolationCount { get; set; }
        public DateTime? LastClearedAt { get; set; }

        /// <summary>24 hourly message-count buckets, as a JSON array string. Nullable so
        /// the client isn't required to send it — the database fills a default on insert.</summary>
        public string? ActivityHistogram { get; set; }

        public DateTime? RetiredAt { get; set; }
        public Guid? MergedIntoTopicId { get; set; }

        public List<Guid> ConsumerIds { get; set; } = new();
        public List<string> TagIds { get; set; } = new();

        /// <summary>Computed on read (see SchemaComplianceValidator) — null if no schema
        /// assigned, else whether the last payload matched every field in the schema.</summary>
        public bool? Compliant { get; set; }

        /// <summary>Human-readable reasons the last payload fails the assigned schema.</summary>
        public List<string> Mismatches { get; set; } = new();

        public TopicDto(Topic topic)
        {
            Id = topic.Id;
            Path = topic.Path;
            Tracked = topic.Tracked;
            ProducerId = topic.ProducerId;
            SchemaId = topic.SchemaId;
            LastPayload = topic.LastPayload;
            LastSeenAt = topic.LastSeenAt;
            Retained = topic.Retained;
            ViolationCount = topic.ViolationCount;
            LastClearedAt = topic.LastClearedAt;
            ActivityHistogram = topic.ActivityHistogram;
            RetiredAt = topic.RetiredAt;
            MergedIntoTopicId = topic.MergedIntoTopicId;
            ConsumerIds = topic.Consumers.Select(c => c.Id).ToList();
            TagIds = topic.Tags.Select(t => t.Id).ToList();
        }

        // Empty constructor for deserialization
        public TopicDto()
        {
        }
    }
}
