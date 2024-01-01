using BlogApp.API.Data;
using BlogApp.API.Models.Domain;
using BlogApp.API.Models.DTO;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using BlogApp.API.Repositories;
using FluentValidation;
using BlogApp.API.Exceptions;

namespace BlogApp.API.Services
{
    public class CategoryService : ICategoryRepository
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IMapper mapper;
        private readonly IValidator<CreateCategoryRequestDto> createCategoryValidator;
        private readonly IValidator<UpdateCategoryRequestDto> updateCategoryValidator;

        public CategoryService(
            ApplicationDbContext dbContext,
            IMapper mapper,
            IValidator<CreateCategoryRequestDto> createCategoryValidator,
            IValidator<UpdateCategoryRequestDto> updateCategoryValidator
        )
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
            this.createCategoryValidator = createCategoryValidator;
            this.updateCategoryValidator = updateCategoryValidator;
        }

        public async Task<PaginatedResult<CategoryDto>> GetAllAsync(int page, int pageSize)
        {
            if (page == 0 || pageSize == 0)
            {
                throw new InvalidOperationException("Page or PageSize cannot be 0");
            }

            try
            {
                pageSize = Math.Min(pageSize, 50);
                var query = dbContext.Categories.AsNoTracking();
                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                if (totalItems != 0 && page > totalPages)
                {
                    throw new BadRequestException($"Page cannot be greater than {totalPages}");
                }

                var categories = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var categoriesDtos = mapper.Map<List<CategoryDto>>(categories);

                var result = new PaginatedResult<CategoryDto>
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    Items = categoriesDtos
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

        public async Task<CategoryDto> GetByIdAsync(Guid id)
        {
            try
            {
                var existingCategory = await dbContext.Categories.FirstOrDefaultAsync(
                    c => c.Id == id
                );
                _ = existingCategory ?? throw new NotFoundException("Category not found.");

                return mapper.Map<CategoryDto>(existingCategory);
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<Category> GetDomainModelByIdAsync(Guid id)
        {
            try
            {
                return await dbContext.Categories.FirstOrDefaultAsync(c => c.Id == id);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(
                    "Failed to retrieve a category domain model by ID.",
                    e
                );
            }
        }

        public async Task<CategoryDto> CreateAsync(CreateCategoryRequestDto request)
        {
            var validation = await createCategoryValidator.ValidateAsync(request);
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
                var category = mapper.Map<Category>(request);
                await dbContext.Categories.AddAsync(category);
                await dbContext.SaveChangesAsync();

                return mapper.Map<CategoryDto>(category);
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<CategoryDto> UpdateAsync(Guid id, UpdateCategoryRequestDto request)
        {
            var validation = await updateCategoryValidator.ValidateAsync(request);
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
                var existingCategory = await dbContext.Categories.FirstOrDefaultAsync(
                    c => c.Id == id
                );
                _ = existingCategory ?? throw new NotFoundException("Category not found.");

                mapper.Map(request, existingCategory);
                await dbContext.SaveChangesAsync();
                return mapper.Map<CategoryDto>(existingCategory);
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
                var existingCategory = await dbContext.Categories.FirstOrDefaultAsync(
                    c => c.Id == id
                );
                _ = existingCategory ?? throw new NotFoundException("Category not found.");

                dbContext.Categories.Remove(existingCategory);
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
