using BlogApp.API.Models.Domain;
using BlogApp.API.Models.DTO;

namespace BlogApp.API.Models.Extensions
{
    public static class BlogPostExtensions
    {
        public static BlogPost ToDomainModel(this CreateBlogPostRequestDto request)
        {
            return new BlogPost
            {
                Author = request.Author,
                Content = request.Content,
                FeaturedImageUrl = request.FeaturedImageUrl,
                IsVisible = request.IsVisible,
                PublishedDate = request.PublishedDate,
                ShortDescription = request.ShortDescription,
                Title = request.Title,
                UrlHandle = request.UrlHandle,
                Categories = new List<Category>()
            };
        }

        public static BlogPostDto ToDto(this BlogPost blogPost)
        {
            return new BlogPostDto
            {
                Id = blogPost.Id,
                Author = blogPost.Author,
                Content = blogPost.Content,
                FeaturedImageUrl = blogPost.FeaturedImageUrl,
                IsVisible = blogPost.IsVisible,
                PublishedDate = blogPost.PublishedDate,
                ShortDescription = blogPost.ShortDescription,
                Title = blogPost.Title,
                UrlHandle = blogPost.UrlHandle,
                Categories = blogPost.Categories
                    .Select(
                        x =>
                            new CategoryDto
                            {
                                Id = x.Id,
                                Name = x.Name,
                                UrlHandle = x.UrlHandle,
                            }
                    )
                    .ToList()
            };
        }

        public static BlogPost ToDomainModel(this UpdateBlogPostRequestDto request)
        {
            return new BlogPost
            {
                // Map the properties from the request DTO to the BlogPost domain model.
                Author = request.Author,
                Content = request.Content,
                FeaturedImageUrl = request.FeaturedImageUrl,
                IsVisible = request.IsVisible,
                PublishedDate = request.PublishedDate,
                ShortDescription = request.ShortDescription,
                Title = request.Title,
                UrlHandle = request.UrlHandle,
                Categories = new List<Category>()
            };
        }
    }
}
