using BlogApp.API.Models.Domain;

namespace BlogApp.API.Repositories.Interfaces
{
    public interface IImageRepository
    {
        Task<IQueryable<BlogImage>> GetAllAsync();
        Task<BlogImage> GetByIdAsync(Guid id);
        Task AddAsync(BlogImage blogImage);
        Task DeleteAsync(BlogImage blogImage);
    }
}
