using BlogApp.API.Models.DTO;

namespace BlogApp.API.Repositories
{
    public interface IBlogPostRepository
    {
        Task<IEnumerable<BlogPostDto>> GetAllAsync();
        Task<BlogPostDto> GetByIdAsync(Guid id);
        Task<BlogPostDto> GetByUrlHandleAsync(string urlHandle);
        Task<BlogPostDto> CreateAsync(CreateBlogPostRequestDto request);
        Task<BlogPostDto> UpdateAsync(Guid id, UpdateBlogPostRequestDto request);
        Task<bool> DeleteAsync(Guid id);
    }
}
