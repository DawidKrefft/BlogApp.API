using AutoMapper;
using BlogApp.API.Data;
using BlogApp.API.Models.Domain;
using BlogApp.API.Models.DTO;
using BlogApp.API.Repositories;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.API.Services
{
    public class BlogPostService : IBlogPostRepository
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IMapper mapper;
        private readonly IValidator<CreateBlogPostRequestDto> createBlogPostValidator;
        private readonly IValidator<UpdateBlogPostRequestDto> updateBlogPostValidator;

        public BlogPostService(
            ApplicationDbContext dbContext,
            IMapper mapper,
            IValidator<CreateBlogPostRequestDto> createBlogPostValidator,
            IValidator<UpdateBlogPostRequestDto> updateBlogPostValidator
        )
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
            this.createBlogPostValidator = createBlogPostValidator;
            this.updateBlogPostValidator = updateBlogPostValidator;
        }

        public async Task<PaginatedResult<BlogPostDto>> GetAllAsync(int page, int pageSize)
        {
            if (page == 0 || pageSize == 0)
            {
                throw new InvalidOperationException("Page or PageSize cannot be 0");
            }

            try
            {
                pageSize = Math.Min(pageSize, 10);

                var query = dbContext.BlogPosts.Include(x => x.Categories).OrderBy(x => x.Id);
                var totalItems = await query.CountAsync();

                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                var blogPosts = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var blogPostDtos = mapper.Map<List<BlogPostDto>>(blogPosts);

                var result = new PaginatedResult<BlogPostDto>
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    Items = blogPostDtos
                };

                return result;
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
            var validation = await createBlogPostValidator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                var validationErrors = string.Join(
                    ", ",
                    validation.Errors.Select(error => error.ErrorMessage)
                );
                throw new ValidationException(validationErrors);
            }
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
                    else
                    {
                        throw new InvalidOperationException($"Category not found.");
                    }
                }

                await dbContext.BlogPosts.AddAsync(blogPost);
                await dbContext.SaveChangesAsync();

                return mapper.Map<BlogPostDto>(blogPost);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message);
            }
        }

        public async Task<BlogPostDto?> UpdateAsync(Guid id, UpdateBlogPostRequestDto request)
        {
            var validation = await updateBlogPostValidator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                var validationErrors = string.Join(
                    ", ",
                    validation.Errors.Select(error => error.ErrorMessage)
                );
                throw new ValidationException(validationErrors);
            }
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
