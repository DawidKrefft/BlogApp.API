using BlogApp.API.Models.Domain;

namespace BlogApp.API.Repositories.Interface
{
    public interface IImageRepository
    {
        Task<BlogImage> Upload(IFormFile file, BlogImage blogImage);
        Task<IEnumerable<BlogImage>> GetAll();
        Task<BlogImage?> DeleteAsync(Guid id);
    }
}
