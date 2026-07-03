using Hively.Server.DbModel;

namespace Hively.Server.Repository.Abstractions
{
    /// <summary>
    /// Repository interface for managing User/UserIdentity entities.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Retrieves all users.
        /// </summary>
        Task<IEnumerable<User>> GetUsersAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves a single user by ID.
        /// </summary>
        /// <exception cref="EntityNotFoundException">Thrown when user is not found.</exception>
        Task<User> GetUserAsync(Guid userId, CancellationToken cancellationToken);

        /// <summary>
        /// Resolves the User for an external login, creating one if needed. Looks up
        /// first by (authProvider, externalSubject); if not found, falls back to
        /// matching by email (linking a new provider to an existing account); only
        /// creates a brand-new user when neither matches. The very first user ever
        /// created is auto-promoted to Admin, every subsequent one defaults to Viewer.
        /// </summary>
        Task<User> UpsertFromExternalLoginAsync(string authProvider, string externalSubject, string email, CancellationToken cancellationToken);

        /// <summary>
        /// Updates a user's role.
        /// </summary>
        /// <exception cref="EntityNotFoundException">Thrown when user is not found.</exception>
        Task<User> UpdateRoleAsync(Guid userId, string role, CancellationToken cancellationToken);
    }
}
