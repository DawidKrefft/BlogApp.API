using AutoMapper;
using BlogApp.API.Data;
using BlogApp.API.Models.Domain;
using BlogApp.API.Models.DTO;
using BlogApp.API.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.API.Services
{
    public class BlogPostService : IBlogPostRepository
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IMapper mapper;

        public BlogPostService(ApplicationDbContext dbContext, IMapper mapper)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<BlogPostDto>> GetAllAsync()
        {
            try
            {
                var blogPosts = await dbContext.BlogPosts.Include(x => x.Categories).ToListAsync();
                return mapper.Map<List<BlogPostDto>>(blogPosts);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve blog posts.", ex);
            }
        }

        public async Task<BlogPostDto?> GetByIdAsync(Guid id)
        {
            try
            {
                var blogPost = await dbContext.BlogPosts
                    .Include(x => x.Categories)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (blogPost != null)
                {
                    return mapper.Map<BlogPostDto>(blogPost);
                }
                else
                {
                    throw new InvalidOperationException("Blog post not found.");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve a blog post by ID.", ex);
            }
        }

        public async Task<BlogPostDto?> GetByUrlHandleAsync(string urlHandle)
        {
            try
            {
                var blogPost = await dbContext.BlogPosts
                    .Include(x => x.Categories)
                    .FirstOrDefaultAsync(x => x.UrlHandle == urlHandle);

                if (blogPost != null)
                {
                    return mapper.Map<BlogPostDto>(blogPost);
                }
                else
                {
                    throw new InvalidOperationException("Blog post not found.");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "Failed to retrieve a blog post by URL handle.",
                    ex
                );
            }
        }

        public async Task<BlogPostDto> CreateAsync(CreateBlogPostRequestDto request)
        {
            try
            {
                var blogPost = mapper.Map<BlogPost>(request);

                foreach (var categoryGuid in request.Categories)
                {
                    var existingCategory = await dbContext.Categories.FirstOrDefaultAsync(
                        c => c.Id == categoryGuid
                    );

                    if (existingCategory != null)
                    {
                        blogPost.Categories.Add(existingCategory);
                    }
                }

                await dbContext.BlogPosts.AddAsync(blogPost);
                await dbContext.SaveChangesAsync();

                return mapper.Map<BlogPostDto>(blogPost);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to create the blog post.", ex);
            }
        }

        public async Task<BlogPostDto?> UpdateAsync(Guid id, UpdateBlogPostRequestDto request)
        {
            try
            {
                var existingBlogPost = await dbContext.BlogPosts
                    .Include(x => x.Categories)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (existingBlogPost != null)
                {
                    mapper.Map(request, existingBlogPost);

                    existingBlogPost.Categories.Clear();

                    foreach (var categoryGuid in request.Categories)
                    {
                        var existingCategory = await dbContext.Categories.FirstOrDefaultAsync(
                            c => c.Id == categoryGuid
                        );

                        if (existingCategory != null)
                        {
                            existingBlogPost.Categories.Add(existingCategory);
                        }
                    }

                    await dbContext.SaveChangesAsync();

                    return mapper.Map<BlogPostDto>(existingBlogPost);
                }
                else
                {
                    throw new InvalidOperationException("Blog post not found.");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to update the blog post.", ex);
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                var existingBlogPost = await dbContext.BlogPosts.SingleOrDefaultAsync(
                    x => x.Id == id
                );

                if (existingBlogPost != null)
                {
                    dbContext.BlogPosts.Remove(existingBlogPost);
                    await dbContext.SaveChangesAsync();
                    return true;
                }
                else
                {
                    throw new InvalidOperationException("Blog post not found.");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to delete the blog post.", ex);
            }
        }
    }
}
