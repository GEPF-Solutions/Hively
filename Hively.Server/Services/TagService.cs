using Hively.Server.Dto;
using Hively.Server.Repository.Abstractions;
using Hively.Server.Services.Abstractions;

namespace Hively.Server.Services
{
    /// <summary>
    /// Service for managing Tag entities.
    /// </summary>
    public class TagService : ITagService
    {
        private readonly ITagRepository _tagRepository;

        public TagService(ITagRepository tagRepository)
        {
            _tagRepository = tagRepository;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TagDto>> GetTagsAsync(CancellationToken cancellationToken)
        {
            var tags = await _tagRepository.GetTagsAsync(cancellationToken);
            return tags.Select(x => new TagDto(x));
        }

        /// <inheritdoc />
        public async Task<TagDto> GetTagAsync(string tagId, CancellationToken cancellationToken)
        {
            var tag = await _tagRepository.GetTagAsync(tagId, cancellationToken);
            return new TagDto(tag);
        }

        /// <inheritdoc />
        public async Task<TagDto> InsertTagAsync(TagDto tag, CancellationToken cancellationToken)
        {
            var createdTag = await _tagRepository.InsertTagAsync(tag, cancellationToken);
            return new TagDto(createdTag);
        }

        /// <inheritdoc />
        public Task UpdateTagAsync(TagDto tag, CancellationToken cancellationToken)
        {
            return _tagRepository.UpdateTagAsync(tag, cancellationToken);
        }

        /// <inheritdoc />
        public Task RemoveTagAsync(string tagId, CancellationToken cancellationToken)
        {
            return _tagRepository.RemoveTagAsync(tagId, cancellationToken);
        }
    }
}
