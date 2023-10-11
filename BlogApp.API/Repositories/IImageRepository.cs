using BlogApp.API.Models.Domain;
using BlogApp.API.Models.DTO;

namespace BlogApp.API.Repositories
{
    public interface IImageRepository
    {
        Task<BlogImageDto> Upload(ImageUploadRequestDto request);
        Task<IEnumerable<BlogImageDto>> GetAll();
        Task<BlogImage> DeleteAsync(Guid id);
    }
}
