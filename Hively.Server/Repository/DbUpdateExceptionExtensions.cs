using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Hively.Server.Repository
{
    /// <summary>
    /// Lets a repository tell a Postgres unique-constraint violation apart from
    /// any other <see cref="DbUpdateException"/> via an exception filter —
    /// <c>catch (DbUpdateException ex) when (ex.IsUniqueViolation())</c> — so it
    /// can throw a clean <see cref="Exceptions.DuplicateEntityException"/> instead
    /// of letting the raw Npgsql exception (and its full SQL text) reach the
    /// controller and, from there, the client.
    /// </summary>
    public static class DbUpdateExceptionExtensions
    {
        public static bool IsUniqueViolation(this DbUpdateException ex)
        {
            return ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
        }
    }
}
