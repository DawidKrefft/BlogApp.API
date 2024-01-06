using BlogApp.API.Models.Domain;
using BlogApp.API.Models.DTO;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using BlogApp.API.Exceptions;
using BlogApp.API.Services.Interfaces;
using BlogApp.API.Repositories.Interfaces;

namespace BlogApp.API.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository categoryRepository;
        private readonly IMapper mapper;
        private readonly IValidator<CreateCategoryRequestDto> createCategoryValidator;
        private readonly IValidator<UpdateCategoryRequestDto> updateCategoryValidator;

        public CategoryService(
            ICategoryRepository categoryRepository,
            IMapper mapper,
            IValidator<CreateCategoryRequestDto> createCategoryValidator,
            IValidator<UpdateCategoryRequestDto> updateCategoryValidator
        )
        {
            this.categoryRepository = categoryRepository;
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
                var query = await categoryRepository.GetAllAsync();
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
                var existingCategory = await categoryRepository.GetByIdAsync(id);
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
                return await categoryRepository.GetByIdAsync(id);
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
                var categoryExists = await categoryRepository.CategoryExistsByNameAsync(
                    request.Name
                );
                _ = categoryExists
                    ? throw new ValidationException(
                        $"Category with name '{request.Name}' already exists."
                    )
                    : categoryExists;

                var category = mapper.Map<Category>(request);
                await categoryRepository.AddAsync(category);

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
                var existingCategory = await categoryRepository.GetByIdAsync(id);
                _ = existingCategory ?? throw new NotFoundException("Category not found.");

                if (request.Name != existingCategory.Name)
                {
                    var categoryExists = await categoryRepository.CategoryExistsByNameAsync(
                        request.Name
                    );
                    _ = categoryExists
                        ? throw new ValidationException(
                            $"Category with name '{request.Name}' already exists."
                        )
                        : categoryExists;
                }

                mapper.Map(request, existingCategory);
                await categoryRepository.UpdateAsync(existingCategory);

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
                var existingCategory = await categoryRepository.GetByIdAsync(id);
                _ = existingCategory ?? throw new NotFoundException("Category not found.");

                await categoryRepository.DeleteAsync(existingCategory);
                return true;
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}
