using Hively.Server.DbModel;
using Hively.Server.Dto;

namespace Hively.Server.Repository.Abstractions
{
    /// <summary>
    /// Repository interface for managing Tag entities.
    /// </summary>
    public interface ITagRepository
    {
        /// <summary>
        /// Retrieves all tags.
        /// </summary>
        Task<IEnumerable<Tag>> GetTagsAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves a single tag by ID (slug).
        /// </summary>
        /// <exception cref="EntityNotFoundException">Thrown when tag is not found.</exception>
        Task<Tag> GetTagAsync(string tagId, CancellationToken cancellationToken);

        /// <summary>
        /// Creates a new tag. The caller supplies the slug id.
        /// </summary>
        Task<Tag> InsertTagAsync(TagDto tag, CancellationToken cancellationToken);

        /// <summary>
        /// Updates an existing tag's label/hue. The id (slug) is immutable.
        /// </summary>
        /// <exception cref="EntityNotFoundException">Thrown when tag is not found.</exception>
        Task UpdateTagAsync(TagDto tag, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes a tag. Topics/Rules referencing it have the association
        /// removed by the database (ON DELETE CASCADE on topic_tags/rule_tags)
        /// — no in-use check here, deletion is never blocked.
        /// </summary>
        /// <exception cref="EntityNotFoundException">Thrown when tag is not found.</exception>
        Task RemoveTagAsync(string tagId, CancellationToken cancellationToken);
    }
}
