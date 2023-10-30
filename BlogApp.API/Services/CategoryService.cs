using BlogApp.API.Data;
using BlogApp.API.Models.Domain;
using BlogApp.API.Models.DTO;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using BlogApp.API.Repositories;
using FluentValidation;

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
            try
            {
                pageSize = Math.Min(pageSize, 50);

                var query = dbContext.Categories.AsNoTracking();
                var totalItems = await query.CountAsync();

                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

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
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve categories.", ex);
            }
        }

        public async Task<CategoryDto> GetByIdAsync(Guid id)
        {
            try
            {
                var existingCategory = await dbContext.Categories.FirstOrDefaultAsync(
                    c => c.Id == id
                );
                if (existingCategory != null)
                {
                    return mapper.Map<CategoryDto>(existingCategory);
                }
                else
                {
                    throw new InvalidOperationException("Category not found.");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve a category by ID.", ex);
            }
        }

        public async Task<Category> GetDomainModelByIdAsync(Guid id)
        {
            try
            {
                return await dbContext.Categories.FirstOrDefaultAsync(c => c.Id == id);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "Failed to retrieve a category domain model by ID.",
                    ex
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
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to create the category.", ex);
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

                if (existingCategory != null)
                {
                    mapper.Map(request, existingCategory);
                    await dbContext.SaveChangesAsync();
                    return mapper.Map<CategoryDto>(existingCategory);
                }
                else
                {
                    throw new InvalidOperationException("Category not found.");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to update the category.", ex);
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                var existingCategory = await dbContext.Categories.FirstOrDefaultAsync(
                    c => c.Id == id
                );

                if (existingCategory != null)
                {
                    dbContext.Categories.Remove(existingCategory);
                    await dbContext.SaveChangesAsync();
                    return true;
                }
                else
                {
                    throw new InvalidOperationException("Category not found.");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to delete the category.", ex);
            }
        }
    }
}
