using BlogApp.API.Data;
using BlogApp.API.Models.Domain;
using BlogApp.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.API.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext dbContext;

        public CategoryRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<IQueryable<Category>> GetAllAsync()
        {
            return await Task.FromResult(dbContext.Categories.AsNoTracking());
        }

        public async Task<Category> GetByIdAsync(Guid id)
        {
            return await dbContext.Categories.SingleOrDefaultAsync(c => c.Id == id);
        }

        public async Task<bool> CategoryExistsByNameAsync(string categoryName)
        {
            return await dbContext.Categories.AnyAsync(c => c.Name == categoryName);
        }

        public async Task AddAsync(Category category)
        {
            await dbContext.Categories.AddAsync(category);
            await dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(Category category)
        {
            dbContext.Categories.Update(category);
            await dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Category category)
        {
            dbContext.Categories.Remove(category);
            await dbContext.SaveChangesAsync();
        }
    }
}
