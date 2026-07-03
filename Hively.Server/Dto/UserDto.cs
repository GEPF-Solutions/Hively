using Hively.Server.DbModel;

namespace Hively.Server.Dto
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;

        /// <summary>"Admin" or "Viewer" — looked up from our own Users table, not the identity provider.</summary>
        public string Role { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public UserDto(User user)
        {
            Id = user.Id;
            Email = user.Email;
            Role = user.Role;
            CreatedAt = user.CreatedAt;
        }

        // Empty constructor for deserialization
        public UserDto()
        {
        }
    }
}
