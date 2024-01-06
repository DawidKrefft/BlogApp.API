using BlogApp.API.Models.Domain;
using BlogApp.API.Models.DTO;

namespace BlogApp.API.Repositories.Interfaces
{
    public interface IBlogPostRepository
    {
        Task<IQueryable<BlogPost>> GetAllWithCategoriesAsync();
        Task<BlogPost> GetByIdAsync(Guid id);
        Task<BlogPost> GetByIdWithCategoriesAsync(Guid id);
        Task<BlogPost> GetByUrlHandleAsync(string urlHandle);
        Task AddAsync(BlogPost blogPost);
        Task UpdateAsync(BlogPost blogPost);
        Task DeleteAsync(BlogPost blogPost);
    }
}
