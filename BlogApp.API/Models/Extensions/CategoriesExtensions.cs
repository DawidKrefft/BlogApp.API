using BlogApp.API.Models.Domain;
using BlogApp.API.Models.DTO;

namespace BlogApp.API.Models.Extensions
{
    public static class CategoryExtensions
    {
        public static Category ToDomainModel(this CreateCategoryRequestDto dto)
        {
            return new Category { Name = dto.Name, UrlHandle = dto.UrlHandle };
        }

        public static CategoryDto ToDto(this Category domainModel)
        {
            return new CategoryDto
            {
                Id = domainModel.Id,
                Name = domainModel.Name,
                UrlHandle = domainModel.UrlHandle
            };
        }

        public static Category ToDomainModel(this UpdateCategoryRequestDto dto)
        {
            return new Category { Name = dto.Name, UrlHandle = dto.UrlHandle };
        }
    }
}
