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
                        Name = "Category1",
                        UrlHandle = "category-1"
                    },
                    new Category
                    {
                        Id = new Guid("E246E480-6CC1-4FE0-ACE5-45D109A95D5F"),
                        Name = "Category2",
                        UrlHandle = "category-2"
                    },
                    new Category
                    {
                        Id = new Guid("A7DFAABD-2E85-4F9E-B9DD-6C784C84B3A2"),
                        Name = "Category3",
                        UrlHandle = "category-3"
                    },
                    new Category
                    {
                        Id = new Guid("F32F819A-67A2-4C95-9E14-792DCAF84A67"),
                        Name = "Category4",
                        UrlHandle = "category-4"
                    },
                    new Category
                    {
                        Id = new Guid("1B24C116-A18F-4949-9F1A-60A88B30BB67"),
                        Name = "Category5",
                        UrlHandle = "category-5"
                    },
                    new Category
                    {
                        Id = new Guid("C5F2C59A-EAC1-44A1-8E64-524EB804A9F2"),
                        Name = "Category6",
                        UrlHandle = "category-6"
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
                        FeaturedImageUrl = "https://localhost:7055/Images/sample1.jpg",
                        UrlHandle = "url-handle1",
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
                        FeaturedImageUrl = "https://localhost:7055/Images/sample2.jpg",
                        UrlHandle = "url-handle2",
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
                        Url = "https://localhost:7055/Images/sample1.jpg",
                        DateCreated = DateTime.Now
                    },
                    new BlogImage
                    {
                        Id = new Guid("36FF9F0B-032F-4EF0-AB44-0F2A8B4716C0"),
                        FileName = "image2.jpg",
                        FileExtension = "jpg",
                        Title = "Sample Image 2",
                        Url = "https://localhost:7055/Images/sample2.jpg",
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
