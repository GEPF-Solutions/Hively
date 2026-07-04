namespace Hively.Server.Dto
{
    /// <summary>Request body for <c>POST /api/auth/login/basic</c>.</summary>
    public class BasicLoginRequestDto
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
