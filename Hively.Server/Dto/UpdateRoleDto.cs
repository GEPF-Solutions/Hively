namespace Hively.Server.Dto
{
    /// <summary>Request shape for promoting/demoting a user (Admin-only action).</summary>
    public class UpdateRoleDto
    {
        public string Role { get; set; } = null!;
    }
}
