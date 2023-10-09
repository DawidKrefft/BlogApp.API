using AutoMapper;
using BlogApp.API.Models.Domain;
using BlogApp.API.Models.DTO;

namespace BlogApp.API.Models.Profiles
{
    public class CategoryProfile : Profile
    {
        public CategoryProfile()
        {
            CreateMap<Category, CategoryDto>();
            CreateMap<CreateCategoryRequestDto, Category>();
            CreateMap<UpdateCategoryRequestDto, Category>();
        }
    }
}
