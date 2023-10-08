using BlogApp.API.Models.Domain;
using BlogApp.API.Models.DTO;

namespace BlogApp.API.Models.Extensions
{
    public static class ImageExtensions
    {
        public static BlogImageDto ToDto(this BlogImage image)
        {
            return new BlogImageDto
            {
                Id = image.Id,
                Title = image.Title,
                DateCreated = image.DateCreated,
                FileExtension = image.FileExtension,
                FileName = image.FileName,
                Url = image.Url
            };
        }
    }
}
