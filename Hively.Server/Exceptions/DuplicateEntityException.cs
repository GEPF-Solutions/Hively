namespace Hively.Server.Exceptions
{
    /// <summary>Thrown when an insert/update would violate a unique constraint (name, path, slug, etc.).</summary>
    public class DuplicateEntityException : Exception
    {
        public DuplicateEntityException(string? message) : base(message)
        {
        }
    }
}
