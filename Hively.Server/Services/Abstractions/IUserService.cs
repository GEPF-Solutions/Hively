using Hively.Server.Dto;

namespace Hively.Server.Services.Abstractions
{
    /// <summary>
    /// Service interface for User business logic.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Retrieves all users.
        /// </summary>
        Task<IEnumerable<UserDto>> GetUsersAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves a single user by ID.
        /// </summary>
        Task<UserDto> GetUserAsync(Guid userId, CancellationToken cancellationToken);

        /// <summary>
        /// Resolves or creates the User for an external login — called from the
        /// Google/Entra auth handlers' post-authentication events (see Program.cs),
        /// and directly from AuthController's basic-auth login action, which has no
        /// handler middleware of its own to hook into.
        /// </summary>
        Task<UserDto> UpsertFromExternalLoginAsync(string authProvider, string externalSubject, string email, CancellationToken cancellationToken);

        /// <summary>
        /// Updates a user's role. Throws <see cref="InvalidOperationException"/> for
        /// any role other than "Admin"/"Viewer".
        /// </summary>
        Task<UserDto> UpdateRoleAsync(Guid userId, string role, CancellationToken cancellationToken);
    }
}
