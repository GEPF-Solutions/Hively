using Hively.Server.Dto;
using Hively.Server.Repository.Abstractions;
using Hively.Server.Services.Abstractions;

namespace Hively.Server.Services
{
    /// <summary>
    /// Service for managing User entities, including external-login resolution.
    /// </summary>
    public class UserService : IUserService
    {
        private static readonly HashSet<string> ValidRoles = new() { "Admin", "Viewer" };

        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<UserDto>> GetUsersAsync(CancellationToken cancellationToken)
        {
            var users = await _userRepository.GetUsersAsync(cancellationToken);
            return users.Select(u => new UserDto(u));
        }

        /// <inheritdoc />
        public async Task<UserDto> GetUserAsync(Guid userId, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetUserAsync(userId, cancellationToken);
            return new UserDto(user);
        }

        /// <inheritdoc />
        public async Task<UserDto> UpsertFromExternalLoginAsync(string authProvider, string externalSubject, string email, CancellationToken cancellationToken)
        {
            var user = await _userRepository.UpsertFromExternalLoginAsync(authProvider, externalSubject, email, cancellationToken);
            return new UserDto(user);
        }

        /// <inheritdoc />
        public async Task<UserDto> UpdateRoleAsync(Guid userId, string role, CancellationToken cancellationToken)
        {
            if (!ValidRoles.Contains(role))
            {
                throw new InvalidOperationException($"'{role}' is not a valid role. Must be one of: {string.Join(", ", ValidRoles)}.");
            }

            var user = await _userRepository.UpdateRoleAsync(userId, role, cancellationToken);
            return new UserDto(user);
        }
    }
}
