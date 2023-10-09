using AutoMapper;
using BlogApp.API.Models.Domain;
using BlogApp.API.Models.DTO;

public class BlogPostProfile : Profile
{
    public BlogPostProfile()
    {
        CreateMap<CreateBlogPostRequestDto, BlogPost>()
            .ForMember(dest => dest.Categories, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        CreateMap<BlogPost, BlogPostDto>()
            .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.Categories));

        CreateMap<UpdateBlogPostRequestDto, BlogPost>()
            .ForMember(dest => dest.Categories, opt => opt.Ignore());

        CreateMap<BlogPost, BlogPostDto>()
            .ForMember(
                dest => dest.Categories,
                opt =>
                    opt.MapFrom(
                        src =>
                            src.Categories.Select(
                                x =>
                                    new CategoryDto
                                    {
                                        Id = x.Id,
                                        Name = x.Name,
                                        UrlHandle = x.UrlHandle
                                    }
                            )
                    )
            );
    }
}
