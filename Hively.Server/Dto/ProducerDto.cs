using Hively.Server.DbModel;

namespace Hively.Server.Dto
{
    public class ProducerDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        public ProducerDto(Producer producer)
        {
            Id = producer.Id;
            Name = producer.Name;
            Description = producer.Description;
        }

        // Empty constructor for deserialization
        public ProducerDto()
        {
        }
    }
}
