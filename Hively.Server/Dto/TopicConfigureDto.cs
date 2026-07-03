namespace Hively.Server.Dto
{
    /// <summary>
    /// Request shape for the "Configure Topic" action (assign producer/schema/consumers/tags,
    /// mark as managed) — deliberately narrower than <see cref="TopicDto"/>, which also carries
    /// ingestion-owned and computed fields that this action never touches.
    /// </summary>
    public class TopicConfigureDto
    {
        public Guid Id { get; set; }
        public bool Tracked { get; set; }
        public Guid? ProducerId { get; set; }
        public Guid? SchemaId { get; set; }
        public List<Guid> ConsumerIds { get; set; } = new();
        public List<string> TagIds { get; set; } = new();
    }
}
