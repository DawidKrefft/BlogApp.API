using BlogApp.API.Models.Domain;
using BlogApp.API.Models.DTO;

namespace BlogApp.API.Services.Interfaces
{
    public interface IImageService
    {
        Task<PaginatedResult<BlogImageDto>> GetAllAsync(int page, int pageSize);
        Task<BlogImageDto> Upload(ImageUploadRequestDto request);
        Task<BlogImage> DeleteAsync(Guid id);
    }
}
