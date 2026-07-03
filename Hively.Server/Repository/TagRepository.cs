using Hively.Server.DbModel;
using Hively.Server.Dto;
using Hively.Server.Exceptions;
using Hively.Server.Repository.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Hively.Server.Repository
{
    /// <summary>
    /// Repository for managing Tag entities in the database.
    /// </summary>
    public class TagRepository : ITagRepository
    {
        private readonly HivelyContext _dbContext;

        public TagRepository(HivelyContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Tag>> GetTagsAsync(CancellationToken cancellationToken)
        {
            return await _dbContext.Tags.AsNoTracking()
                .OrderBy(x => x.Label)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Tag> GetTagAsync(string tagId, CancellationToken cancellationToken)
        {
            var tag = await _dbContext.Tags.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == tagId, cancellationToken);

            if (tag == null)
            {
                throw new EntityNotFoundException($"Tag id {tagId} did not reference a valid tag.");
            }

            return tag;
        }

        /// <inheritdoc />
        public async Task<Tag> InsertTagAsync(TagDto tagDto, CancellationToken cancellationToken)
        {
            var newTag = new Tag
            {
                Id = tagDto.Id,
                Label = tagDto.Label,
                Hue = tagDto.Hue
            };

            await _dbContext.Tags.AddAsync(newTag, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return newTag;
        }

        /// <inheritdoc />
        public async Task UpdateTagAsync(TagDto tagDto, CancellationToken cancellationToken)
        {
            var tagToUpdate = await _dbContext.Tags
                .FirstOrDefaultAsync(x => x.Id == tagDto.Id, cancellationToken);

            if (tagToUpdate == null)
            {
                throw new EntityNotFoundException($"Tag id {tagDto.Id} did not reference a valid tag.");
            }

            tagToUpdate.Label = tagDto.Label;
            tagToUpdate.Hue = tagDto.Hue;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task RemoveTagAsync(string tagId, CancellationToken cancellationToken)
        {
            var tagToRemove = await _dbContext.Tags
                .FirstOrDefaultAsync(x => x.Id == tagId, cancellationToken);

            if (tagToRemove == null)
            {
                throw new EntityNotFoundException($"Tag id {tagId} did not reference a valid tag.");
            }

            _dbContext.Tags.Remove(tagToRemove);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
