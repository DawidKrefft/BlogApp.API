using BlogApp.API.Models.Domain;
using BlogApp.API.Models.DTO;

namespace BlogApp.API.Repositories
{
    public interface IImageRepository
    {
        Task<PaginatedResult<BlogImageDto>> GetAllAsync(int page, int pageSize);
        Task<BlogImageDto> Upload(ImageUploadRequestDto request);
        Task<BlogImage> DeleteAsync(Guid id);
    }
}
