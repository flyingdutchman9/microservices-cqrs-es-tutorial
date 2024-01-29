using Microsoft.EntityFrameworkCore;
using Post.Query.Domain.Entities;
using Post.Query.Domain.Repositories;
using Post.Query.Infrastructure.DatabaseAccess;

namespace Post.Query.Infrastructure.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly DatabaseContextFactory _contextFactory;

        public PostRepository(DatabaseContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<bool> CreateAsync(PostEntity entity)
        {
            try
            {
                using DatabaseContext context = _contextFactory.CreateDbContext();
                context.Posts.Add(entity);
                // Discard operator govori da nas ne zanima povratni tip (u ovom slučaju broj zapisa)
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                string innerMessage = ex.InnerException?.Message;

                if (innerMessage?.Contains("Violation of PRIMARY KEY constraint") == true) 
                {
                    return true; // Zeznuo sam sam sebe pa je podatak zapisan, ali Kafka nije dobio info pa consumeru kažem da se javi Kafki
                }

                return false;
            }

            return true;
        }

        public async Task DeleteAsync(Guid postId)
        {
            using DatabaseContext context = _contextFactory.CreateDbContext();
            var post = await GetByIdAsync(postId);

            if (post != null) 
            { 
                context.Posts.Remove(post);
                _ = await context.SaveChangesAsync();
            }
        }

        public async Task<List<PostEntity>> GetAllAsync()
        {
            using DatabaseContext context = _contextFactory.CreateDbContext();
            return await context.Posts.AsNoTracking()
                                .Include(c => c.Comments).AsNoTracking()
                                .ToListAsync();
        }

        public async Task<List<PostEntity>> GetByAuthorAsync(string author)
        {
            using DatabaseContext context = _contextFactory.CreateDbContext();
            return await context.Posts.AsNoTracking()
                                .Include(c => c.Comments).AsNoTracking()
                                .Where(x =>  x.Author == author)
                                .ToListAsync();
        }

        public async Task<PostEntity> GetByIdAsync(Guid postId)
        {
            using DatabaseContext context = _contextFactory.CreateDbContext();
            return await context.Posts
                        //.Include(p => p.Comments)
                        .FirstOrDefaultAsync(x => x.PostId == postId);
        }

        public async Task<List<PostEntity>> GetWithCommentsAsync()
        {
            using DatabaseContext context = _contextFactory.CreateDbContext();
            return await context.Posts.AsNoTracking()
                                .Include(c => c.Comments).AsNoTracking()
                                .Where(x => x.Comments != null && x.Comments.Any() == true)
                                .ToListAsync();
        }

        public async Task<List<PostEntity>> GetWithLikesAsync(int numberOfLikes)
        {
            using DatabaseContext context = _contextFactory.CreateDbContext();
            return await context.Posts.AsNoTracking()
                                .Include(c => c.Comments).AsNoTracking()
                                .Where(x => x.Likes >= numberOfLikes)
                                .ToListAsync();
        }

        public async Task UpdateAsync(PostEntity entity)
        {
            using DatabaseContext context = _contextFactory.CreateDbContext();
            context.Posts.Update(entity);
            _ = await context.SaveChangesAsync();
        }
    }
}
