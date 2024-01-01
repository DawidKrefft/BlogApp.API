using AutoMapper;
using BlogApp.API.Data;
using BlogApp.API.Exceptions;
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
                throw new BadRequestException("Page or PageSize cannot be 0");
            }

            try
            {
                pageSize = Math.Min(pageSize, 10);
                var query = dbContext.BlogPosts.Include(x => x.Categories).OrderBy(x => x.Id);
                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                if (totalItems != 0 && page > totalPages)
                {
                    throw new BadRequestException($"Page cannot be greater than {totalPages}");
                }

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
            catch (BadRequestException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Unexpected error occurred", e);
            }
        }

        public async Task<BlogPostDto> GetByIdAsync(Guid id)
        {
            try
            {
                var blogPost = await dbContext.BlogPosts
                    .Include(x => x.Categories)
                    .FirstOrDefaultAsync(x => x.Id == id);
                _ = blogPost ?? throw new NotFoundException("Blog post not found.");

                return mapper.Map<BlogPostDto>(blogPost);
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<BlogPostDto> GetByUrlHandleAsync(string urlHandle)
        {
            try
            {
                var blogPost = await dbContext.BlogPosts
                    .Include(x => x.Categories)
                    .FirstOrDefaultAsync(x => x.UrlHandle == urlHandle);
                _ = blogPost ?? throw new NotFoundException("Blog post not found.");

                return mapper.Map<BlogPostDto>(blogPost);
            }
            catch (Exception e)
            {
                throw;
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
                    _ = existingCategory ?? throw new NotFoundException("Category not found.");

                    blogPost.Categories.Add(existingCategory);
                }

                await dbContext.BlogPosts.AddAsync(blogPost);
                await dbContext.SaveChangesAsync();

                return mapper.Map<BlogPostDto>(blogPost);
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<BlogPostDto> UpdateAsync(Guid id, UpdateBlogPostRequestDto request)
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
                _ = existingBlogPost ?? throw new NotFoundException("Blog post not found.");

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
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                var existingBlogPost = await dbContext.BlogPosts.SingleOrDefaultAsync(
                    x => x.Id == id
                );
                _ = existingBlogPost ?? throw new NotFoundException("Blog post not found.");

                dbContext.BlogPosts.Remove(existingBlogPost);
                await dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}
