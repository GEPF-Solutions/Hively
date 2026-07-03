using Hively.Server.DbModel;

namespace Hively.Server.Dto
{
    public class ConsumerDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        public ConsumerDto(Consumer consumer)
        {
            Id = consumer.Id;
            Name = consumer.Name;
            Description = consumer.Description;
        }

        // Empty constructor for deserialization
        public ConsumerDto()
        {
        }
    }
}
