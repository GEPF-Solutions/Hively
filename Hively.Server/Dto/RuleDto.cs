using Hively.Server.DbModel;

namespace Hively.Server.Dto
{
    public class RuleDto
    {
        public Guid Id { get; set; }

        /// <summary>Optional label — falls back to showing the pattern when not set.</summary>
        public string? Name { get; set; }

        /// <summary>MQTT-style topic filter using '+' and '#' wildcards.</summary>
        public string Pattern { get; set; } = null!;

        public Guid? ProducerId { get; set; }

        public List<string> TagIds { get; set; } = new();

        public RuleDto(Rule rule)
        {
            Id = rule.Id;
            Name = rule.Name;
            Pattern = rule.Pattern;
            ProducerId = rule.ProducerId;
            TagIds = rule.Tags.Select(t => t.Id).ToList();
        }

        // Empty constructor for deserialization
        public RuleDto()
        {
        }
    }
}
