using BlogApp.API.Data;
using BlogApp.API.Models.Domain;
using BlogApp.API.Models.DTO;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using BlogApp.API.Repositories;

namespace BlogApp.API.Services
{
    public class CategoryService : ICategoryRepository
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IMapper mapper;

        public CategoryService(ApplicationDbContext dbContext, IMapper mapper)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllAsync()
        {
            try
            {
                var categories = await dbContext.Categories.ToListAsync();
                return mapper.Map<List<CategoryDto>>(categories);
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

        public async Task<Category?> GetDomainModelByIdAsync(Guid id)
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
