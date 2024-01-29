using Post.Query.Domain.Entities;

namespace Post.Query.Domain.Repositories
{
    public interface IPostRepository
    {
        Task<bool> CreateAsync(PostEntity entity);
        Task UpdateAsync(PostEntity entity);
        Task DeleteAsync(Guid postId);
        Task<PostEntity> GetByIdAsync(Guid postId);
        Task<List<PostEntity>> GetAllAsync();
        Task<List<PostEntity>> GetByAuthorAsync(string author);
        Task<List<PostEntity>> GetWithLikesAsync(int numberOfLikes);
        Task<List<PostEntity>> GetWithCommentsAsync();
    }
}
