using Hively.Server.DbModel;
using Hively.Server.Exceptions;
using Hively.Server.Repository.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Hively.Server.Repository
{
    /// <summary>
    /// Repository for managing User/UserIdentity entities in the database.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly HivelyContext _dbContext;

        public UserRepository(HivelyContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<User>> GetUsersAsync(CancellationToken cancellationToken)
        {
            return await _dbContext.Users
                .AsNoTracking()
                .OrderBy(u => u.Email)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<User> GetUserAsync(Guid userId, CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user == null)
            {
                throw new EntityNotFoundException($"User id {userId} did not reference a valid user.");
            }

            return user;
        }

        /// <inheritdoc />
        public async Task<User> UpsertFromExternalLoginAsync(string authProvider, string externalSubject, string email, CancellationToken cancellationToken)
        {
            var identity = await _dbContext.UserIdentities
                .Include(i => i.User)
                .FirstOrDefaultAsync(i => i.AuthProvider == authProvider && i.ExternalSubject == externalSubject, cancellationToken);

            if (identity != null)
            {
                return identity.User;
            }

            var existingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
            if (existingUser != null)
            {
                _dbContext.UserIdentities.Add(new UserIdentity
                {
                    UserId = existingUser.Id,
                    AuthProvider = authProvider,
                    ExternalSubject = externalSubject
                });

                await _dbContext.SaveChangesAsync(cancellationToken);
                return existingUser;
            }

            var isFirstUser = !await _dbContext.Users.AnyAsync(cancellationToken);
            var newUser = new User
            {
                Email = email,
                Role = isFirstUser ? "Admin" : "Viewer"
            };
            newUser.UserIdentities.Add(new UserIdentity
            {
                AuthProvider = authProvider,
                ExternalSubject = externalSubject
            });

            await _dbContext.Users.AddAsync(newUser, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return newUser;
        }

        /// <inheritdoc />
        public async Task<User> UpdateRoleAsync(Guid userId, string role, CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
            if (user == null)
            {
                throw new EntityNotFoundException($"User id {userId} did not reference a valid user.");
            }

            user.Role = role;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return user;
        }
    }
}
