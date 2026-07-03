using Hively.Server.DbModel;

namespace Hively.Server.Dto
{
    public class TagDto
    {
        public string Id { get; set; } = null!;
        public string Label { get; set; } = null!;
        public int? Hue { get; set; }

        public TagDto(Tag tag)
        {
            Id = tag.Id;
            Label = tag.Label;
            Hue = tag.Hue;
        }

        // Empty constructor for deserialization
        public TagDto()
        {
        }
    }
}
