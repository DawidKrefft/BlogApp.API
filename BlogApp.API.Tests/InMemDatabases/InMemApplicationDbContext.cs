using BlogApp.API.Data;
using BlogApp.API.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.API.Tests.InMemDatabases
{
    public class InMemApplicationDbContext
    {
        public async Task<ApplicationDbContext> GetDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var databaseContext = new ApplicationDbContext(options);
            databaseContext.Database.EnsureCreated();

            if (await databaseContext.Categories.CountAsync() <= 0)
            {
                var categories = new List<Category>
                {
                    new Category
                    {
                        Id = new Guid("B0385524-D562-48AC-8A39-08DBC68D59C0"),
                        Name = "Category 1",
                        UrlHandle = "category-1"
                    },
                    new Category
                    {
                        Id = new Guid("E246E480-6CC1-4FE0-ACE5-45D109A95D5F"),
                        Name = "Category 2",
                        UrlHandle = "category-2"
                    },
                };

                databaseContext.Categories.AddRange(categories);
                databaseContext.SaveChanges();
            }

            if (await databaseContext.BlogPosts.CountAsync() <= 0)
            {
                var categories = databaseContext.Categories.ToList();

                var blogPosts = new List<BlogPost>
                {
                    new BlogPost
                    {
                        Id = new Guid("1E3279E3-4810-41C8-831C-3E0A10C4EAC3"),
                        Title = "Sample Blog Post 1",
                        ShortDescription = "This is a sample blog post 1.",
                        Content = "Lorem ipsum...",
                        PublishedDate = DateTime.Now,
                        Author = "John Doe",
                        IsVisible = true,
                        Categories = new List<Category> { categories[0] }
                    },
                    new BlogPost
                    {
                        Id = new Guid("2D8BC2F3-A91D-472E-AAD0-9A5E15A7A7AA"),
                        Title = "Sample Blog Post 2",
                        ShortDescription = "This is a sample blog post 2.",
                        Content = "Lorem ipsum...",
                        PublishedDate = DateTime.Now,
                        Author = "Jane Smith",
                        IsVisible = true,
                        Categories = new List<Category> { categories[1] }
                    },
                };

                databaseContext.BlogPosts.AddRange(blogPosts);
                databaseContext.SaveChanges();
            }

            if (await databaseContext.BlogImages.CountAsync() <= 0)
            {
                var blogImages = new List<BlogImage>
                {
                    new BlogImage
                    {
                        Id = new Guid("0671EA85-1710-4EAB-8BCA-89ACCCBABE27"),
                        FileName = "image1.jpg",
                        FileExtension = "jpg",
                        Title = "Sample Image 1",
                        Url = "/images/image1.jpg",
                        DateCreated = DateTime.Now
                    },
                    new BlogImage
                    {
                        Id = new Guid("36FF9F0B-032F-4EF0-AB44-0F2A8B4716C0"),
                        FileName = "image2.jpg",
                        FileExtension = "jpg",
                        Title = "Sample Image 2",
                        Url = "/images/image2.jpg",
                        DateCreated = DateTime.Now
                    },
                };

                databaseContext.BlogImages.AddRange(blogImages);
                databaseContext.SaveChanges();
            }

            return databaseContext;
        }
    }
}
