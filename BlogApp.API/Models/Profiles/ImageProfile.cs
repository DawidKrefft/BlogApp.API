using AutoMapper;
using BlogApp.API.Models.Domain;
using BlogApp.API.Models.DTO;

public class ImageProfile : Profile
{
    public ImageProfile()
    {
        CreateMap<BlogImage, BlogImageDto>();
    }
}
