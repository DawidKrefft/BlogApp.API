using BlogApp.API.Models.Domain;

namespace BlogApp.API.Repositories.Interfaces
{
    public interface ICategoryRepository
    {
        Task<IQueryable<Category>> GetAllAsync();
        Task<Category> GetByIdAsync(Guid id);
        Task<bool> CategoryExistsByNameAsync(string categoryName);
        Task AddAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(Category category);
    }
}
