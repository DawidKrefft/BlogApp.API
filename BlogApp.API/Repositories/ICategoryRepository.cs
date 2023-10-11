using BlogApp.API.Models.Domain;
using BlogApp.API.Models.DTO;

namespace BlogApp.API.Repositories
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<CategoryDto>> GetAllAsync();

        Task<CategoryDto> GetByIdAsync(Guid id);

        Task<Category> GetDomainModelByIdAsync(Guid id);

        Task<CategoryDto> CreateAsync(CreateCategoryRequestDto request);

        Task<CategoryDto> UpdateAsync(Guid id, UpdateCategoryRequestDto request);

        Task<bool> DeleteAsync(Guid id);
    }
}
