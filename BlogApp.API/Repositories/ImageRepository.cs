using BlogApp.API.Data;
using BlogApp.API.Models.Domain;
using BlogApp.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.API.Repositories
{
    public class ImageRepository : IImageRepository
    {
        private readonly ApplicationDbContext dbContext;

        public ImageRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<IQueryable<BlogImage>> GetAllAsync()
        {
            return await Task.FromResult(dbContext.BlogImages.AsNoTracking());
        }

        public async Task<BlogImage> GetByIdAsync(Guid id)
        {
            return await dbContext.BlogImages.FirstOrDefaultAsync(image => image.Id == id);
        }

        public async Task AddAsync(BlogImage blogImage)
        {
            await dbContext.BlogImages.AddAsync(blogImage);
            await dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(BlogImage blogImage)
        {
            dbContext.BlogImages.Remove(blogImage);
            await dbContext.SaveChangesAsync();
        }
    }
}
