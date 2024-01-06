using AutoMapper;
using BlogApp.API.Exceptions;
using BlogApp.API.Models.Domain;
using BlogApp.API.Models.DTO;
using BlogApp.API.Repositories.Interfaces;
using BlogApp.API.Services.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.API.Services
{
    public class BlogPostService : IBlogPostService
    {
        private readonly IBlogPostRepository blogPostRepository;
        private readonly ICategoryRepository categoryRepository;
        private readonly IMapper mapper;
        private readonly IValidator<CreateBlogPostRequestDto> createBlogPostValidator;
        private readonly IValidator<UpdateBlogPostRequestDto> updateBlogPostValidator;

        public BlogPostService(
            IBlogPostRepository blogPostRepository,
            ICategoryRepository categoryRepository,
            IMapper mapper,
            IValidator<CreateBlogPostRequestDto> createBlogPostValidator,
            IValidator<UpdateBlogPostRequestDto> updateBlogPostValidator
        )
        {
            this.blogPostRepository = blogPostRepository;
            this.categoryRepository = categoryRepository;
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
                var query = await blogPostRepository.GetAllWithCategoriesAsync();
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
                var blogPost = await blogPostRepository.GetByIdWithCategoriesAsync(id);
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
                var blogPost = await blogPostRepository.GetByUrlHandleAsync(urlHandle);
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
                    var existingCategory = await categoryRepository.GetByIdAsync(categoryGuid);
                    _ = existingCategory ?? throw new NotFoundException("Category not found.");

                    blogPost.Categories.Add(existingCategory);
                }
                await blogPostRepository.AddAsync(blogPost);

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
                var existingBlogPost = await blogPostRepository.GetByIdWithCategoriesAsync(id);
                _ = existingBlogPost ?? throw new NotFoundException("Blog post not found.");

                mapper.Map(request, existingBlogPost);

                existingBlogPost.Categories.Clear();

                foreach (var categoryGuid in request.Categories)
                {
                    var existingCategory = await categoryRepository.GetByIdAsync(categoryGuid);
                    if (existingCategory != null)
                    {
                        existingBlogPost.Categories.Add(existingCategory);
                    }
                }
                await blogPostRepository.UpdateAsync(existingBlogPost);

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
                var existingBlogPost = await blogPostRepository.GetByIdAsync(id);
                _ = existingBlogPost ?? throw new NotFoundException("Blog post not found.");

                await blogPostRepository.DeleteAsync(existingBlogPost);
                return true;
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}
