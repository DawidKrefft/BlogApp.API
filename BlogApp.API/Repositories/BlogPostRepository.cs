using AutoMapper;
using BlogApp.API.Data;
using BlogApp.API.Exceptions;
using BlogApp.API.Models.Domain;
using BlogApp.API.Models.DTO;
using BlogApp.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.API.Repositories
{
    public class BlogPostRepository : IBlogPostRepository
    {
        private readonly ApplicationDbContext dbContext;

        public BlogPostRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<IQueryable<BlogPost>> GetAllWithCategoriesAsync()
        {
            return await Task.FromResult(
                dbContext.BlogPosts.Include(x => x.Categories).OrderBy(x => x.Id)
            );
        }

        public async Task<BlogPost> GetByIdAsync(Guid id)
        {
            return await dbContext.BlogPosts.SingleOrDefaultAsync(b => b.Id == id);
        }

        public async Task<BlogPost> GetByIdWithCategoriesAsync(Guid id)
        {
            return await dbContext.BlogPosts
                .Include(x => x.Categories)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<BlogPost> GetByUrlHandleAsync(string urlHandle)
        {
            return await dbContext.BlogPosts
                .Include(x => x.Categories)
                .FirstOrDefaultAsync(x => x.UrlHandle == urlHandle);
        }

        public async Task AddAsync(BlogPost blogPost)
        {
            await dbContext.BlogPosts.AddAsync(blogPost);
            await dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(BlogPost blogPost)
        {
            dbContext.BlogPosts.Update(blogPost);
            await dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(BlogPost blogPost)
        {
            dbContext.BlogPosts.Remove(blogPost);
            await dbContext.SaveChangesAsync();
        }
    }
}
