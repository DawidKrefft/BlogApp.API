using BlogApp.API.Data;
using BlogApp.API.IntegrationTests.Helpers;
using BlogApp.API.Models.Domain;
using BlogApp.API.Models.DTO;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace BlogApp.API.IntegrationTests.Controllers
{
    public class BlogPostsControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly WebApplicationFactory<Program> _factory;
        private readonly ITestOutputHelper _output;

        public BlogPostsControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var dbContextOptions = services.SingleOrDefault(
                        service =>
                            service.ServiceType == typeof(DbContextOptions<ApplicationDbContext>)
                    );

                    services.Remove(dbContextOptions);

                    services.AddSingleton<IPolicyEvaluator, FakePolicyEvaluator>();

                    services.AddMvc(option => option.Filters.Add(new FakeUserFilter()));

                    services.AddDbContext<ApplicationDbContext>(
                        options => options.UseInMemoryDatabase("ApplicationDb")
                    );
                });
            });

            _client = _factory.CreateClient();
        }

        private void SeedBlogPost(BlogPost blogPost)
        {
            var scopeFactory = _factory.Services.GetService<IServiceScopeFactory>();
            using var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

            dbContext.BlogPosts.Add(blogPost);
            dbContext.SaveChanges();
        }

        private void SeedCategory(Category category)
        {
            var scopeFactory = _factory.Services.GetService<IServiceScopeFactory>();
            using var scope = scopeFactory.CreateScope();
            var _dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

            _dbContext.Categories.Add(category);
            _dbContext.SaveChanges();
        }

        // **** //
        // Get  //
        // **** //

        [Theory]
        [InlineData("page=1&pageSize=5")]
        [InlineData("page=2&pageSize=15")]
        [InlineData("page=3&pageSize=10")]
        [InlineData(null)]
        [InlineData("")]
        public async Task GetAllBlogPost_WithQueryParameters_ReturnsOkResult(string queryParams)
        {
            // Arrange
            var response = await _client.GetAsync("/api/blogposts?" + queryParams);

            // Act & Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Theory]
        [InlineData("page=2&pageSize=1")]
        [InlineData("page=300&pageSize=11")]
        public async Task GetAllBlogPost_WithInvalidQueryParams_ReturnsBadRequest(
            string queryParams
        )
        {
            // Arrange
            var category = new Category { Name = "test1", UrlHandle = "test-1" };

            var blogPost = new BlogPost()
            {
                Title = "Sample Blog Post 1",
                ShortDescription = "This is a sample blog post 1.",
                Content = "Lorem ipsum...",
                FeaturedImageUrl = "https://localhost:7055/Images/sample1.jpg",
                UrlHandle = "url-handle1",
                PublishedDate = new DateTime(2023, 1, 1),
                Author = "John Doe",
                IsVisible = true,
                Categories = new List<Category> { category }
            };
            SeedBlogPost(blogPost);

            // Act
            var response = await _client.GetAsync("/api/blogposts?" + queryParams);

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetBlogPostById_ReturnsOk()
        {
            // Arrange
            var category = new Category { Name = "test1", UrlHandle = "test-1" };

            var existingBlogPost = new BlogPost()
            {
                Title = "Sample Blog Post 1",
                ShortDescription = "This is a sample blog post 1.",
                Content = "Lorem ipsum...",
                FeaturedImageUrl = "https://localhost:7055/Images/sample1.jpg",
                UrlHandle = "url-handle1",
                PublishedDate = new DateTime(2023, 1, 1),
                Author = "John Doe",
                IsVisible = true,
                Categories = new List<Category> { category }
            };
            SeedBlogPost(existingBlogPost);

            // Act
            var response = await _client.GetAsync("/api/blogposts/" + existingBlogPost.Id);

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetBlogPostById_ForNonExistingBlogPost_ReturnsNotFound()
        {
            // Arrange
            var nonExistingBlogPostId = new Guid("3b72c160-87f4-4ee2-aa80-16c6a3d51d86");

            // Act
            var response = await _client.GetAsync("/api/blogposts/" + nonExistingBlogPostId);

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetBlogPostByUrlHandle_ReturnsOk()
        {
            // Arrange
            var category = new Category { Name = "test1", UrlHandle = "test-1" };

            var existingBlogPost = new BlogPost()
            {
                Title = "Sample Blog Post 1",
                ShortDescription = "This is a sample blog post 1.",
                Content = "Lorem ipsum...",
                FeaturedImageUrl = "https://localhost:7055/Images/sample1.jpg",
                UrlHandle = "url-handle1",
                PublishedDate = new DateTime(2023, 1, 1),
                Author = "John Doe",
                IsVisible = true,
                Categories = new List<Category> { category }
            };
            SeedBlogPost(existingBlogPost);

            // Act
            var response = await _client.GetAsync("/api/blogposts/" + existingBlogPost.UrlHandle);

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetBlogPostByUrlHandle_ForNonExistingBlogPost_ReturnsNotFound()
        {
            // Arrange
            var nonExistingBlogPostUrlHandle = "non-existing-post";

            // Act
            var response = await _client.GetAsync("/api/blogposts/" + nonExistingBlogPostUrlHandle);

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        // ******* //
        // Create  //
        // ******* //

        [Fact]
        public async Task CreateBlogPost_WithValidModel_ReturnsCreatedStatus()
        {
            // Arrange
            var category = new Category() { Name = "test1", UrlHandle = "test-1" };
            SeedCategory(category);

            var createBlogPostDto = new CreateBlogPostRequestDto
            {
                Title = "Sample Blog Post 1",
                ShortDescription = "This is a sample blog post 1.",
                Content = "Lorem ipsum...",
                FeaturedImageUrl = "https://localhost:7055/Images/sample1.jpg",
                UrlHandle = "url-handle1",
                PublishedDate = new DateTime(2023, 1, 1),
                Author = "John Doe",
                IsVisible = true,
                Categories = new List<Guid> { category.Id }
            };
            var httpContent = createBlogPostDto.ToJsonHttpContent();

            // Act
            var response = await _client.PostAsync("/api/blogposts", httpContent);

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateBlogPost_WithInvalidModel_ReturnsBadRequest()
        {
            // Arrange
            var createBlogPostDto = new CreateBlogPostRequestDto
            {
                Title = "New Post",
                Content = "New Content"
                // Missing required properties to make it invalid
            };
            var httpContent = createBlogPostDto.ToJsonHttpContent();

            // Act
            var response = await _client.PostAsync("/api/blogposts", httpContent);

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        // ******* //
        // Update  //
        // ******* //

        [Fact]
        public async Task UpdateBlogPostById_WithValidModel_ReturnsOk()
        {
            // Arrange
            var category = new Category { Name = "test1", UrlHandle = "test-1" };

            var existingBlogPost = new BlogPost()
            {
                Title = "Sample Blog Post 1",
                ShortDescription = "This is a sample blog post 1.",
                Content = "Lorem ipsum...",
                FeaturedImageUrl = "https://localhost:7055/Images/sample1.jpg",
                UrlHandle = "url-handle1",
                PublishedDate = new DateTime(2023, 1, 1),
                Author = "John Doe",
                IsVisible = true,
                Categories = new List<Category> { category }
            };
            SeedBlogPost(existingBlogPost);

            var updateBlogPostDto = new UpdateBlogPostRequestDto
            {
                Title = "Updated Post",
                ShortDescription = "Updated Short Description",
                Content = "Updated Content",
                FeaturedImageUrl = "https://localhost:7055/Images/updated.jpg",
                UrlHandle = "updated-url-handle",
                PublishedDate = new DateTime(2023, 2, 1),
                Author = "Updated Author",
                IsVisible = false
            };
            var httpContent = updateBlogPostDto.ToJsonHttpContent();

            // Act
            var response = await _client.PutAsync(
                "/api/blogposts/" + existingBlogPost.Id,
                httpContent
            );

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task UpdateBlogPostById_WithInvalidModel_ReturnsBadRequest()
        {
            // Arrange
            var category = new Category { Name = "test1", UrlHandle = "test-1" };

            var existingBlogPost = new BlogPost()
            {
                Title = "Sample Blog Post 1",
                ShortDescription = "This is a sample blog post 1.",
                Content = "Lorem ipsum...",
                FeaturedImageUrl = "https://localhost:7055/Images/sample1.jpg",
                UrlHandle = "url-handle1",
                PublishedDate = new DateTime(2023, 1, 1),
                Author = "John Doe",
                IsVisible = true,
                Categories = new List<Category> { category }
            };
            SeedBlogPost(existingBlogPost);

            var updateBlogPostDto = new UpdateBlogPostRequestDto
            {
                Title = "Updated Post",
                Content = "Updated Content"
                // Missing required properties to make it invalid
            };
            var httpContent = updateBlogPostDto.ToJsonHttpContent();

            // Act
            var response = await _client.PutAsync(
                "/api/blogposts/" + existingBlogPost.Id,
                httpContent
            );

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        // ******* //
        // Delete  //
        // ******* //

        [Fact]
        public async Task DeleteBlogPost_ReturnsNoContent()
        {
            // Arrange
            var category = new Category { Name = "test1", UrlHandle = "test-1" };

            var existingBlogPost = new BlogPost()
            {
                Title = "Sample Blog Post 1",
                ShortDescription = "This is a sample blog post 1.",
                Content = "Lorem ipsum...",
                FeaturedImageUrl = "https://localhost:7055/Images/sample1.jpg",
                UrlHandle = "url-handle1",
                PublishedDate = new DateTime(2023, 1, 1),
                Author = "John Doe",
                IsVisible = true,
                Categories = new List<Category> { category }
            };
            SeedBlogPost(existingBlogPost);

            // Act
            var response = await _client.DeleteAsync("/api/blogposts/" + existingBlogPost.Id);

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task DeleteBlogPost_ForNonExistingBlogPost_ReturnsNotFound()
        {
            // Arrange
            var nonExistingBlogPostId = new Guid("3b72c160-87f4-4ee2-aa80-16c6a3d51d86");

            // Act
            var response = await _client.DeleteAsync("/api/blogposts/" + nonExistingBlogPostId);

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }
    }
}
