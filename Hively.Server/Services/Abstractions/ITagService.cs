using Hively.Server.Dto;

namespace Hively.Server.Services.Abstractions
{
    /// <summary>
    /// Service interface for Tag business logic.
    /// </summary>
    public interface ITagService
    {
        /// <summary>
        /// Retrieves all tags.
        /// </summary>
        Task<IEnumerable<TagDto>> GetTagsAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves a single tag by ID (slug).
        /// </summary>
        Task<TagDto> GetTagAsync(string tagId, CancellationToken cancellationToken);

        /// <summary>
        /// Creates a new tag.
        /// </summary>
        Task<TagDto> InsertTagAsync(TagDto tag, CancellationToken cancellationToken);

        /// <summary>
        /// Updates an existing tag.
        /// </summary>
        Task UpdateTagAsync(TagDto tag, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes a tag.
        /// </summary>
        Task RemoveTagAsync(string tagId, CancellationToken cancellationToken);
    }
}
