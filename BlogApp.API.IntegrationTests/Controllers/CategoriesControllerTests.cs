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
    public class CategoriesControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private HttpClient _client;
        private WebApplicationFactory<Program> _factory;
        private readonly ITestOutputHelper _output;

        public CategoriesControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var dbContextOptions = services.SingleOrDefault(
                        service =>
                            service.ServiceType == typeof(DbContextOptions<ApplicationDbContext>)
                    );
                    var dbContextAuthOptions = services.SingleOrDefault(
                        service => service.ServiceType == typeof(DbContextOptions<AuthDbContext>)
                    );

                    services.Remove(dbContextOptions);
                    services.Remove(dbContextAuthOptions);

                    services.AddSingleton<IPolicyEvaluator, FakePolicyEvaluator>();

                    services.AddMvc(option => option.Filters.Add(new FakeUserFilter()));

                    services.AddDbContext<ApplicationDbContext>(
                        options => options.UseInMemoryDatabase("ApplicationDb")
                    );
                    services.AddDbContext<AuthDbContext>(
                        options => options.UseInMemoryDatabase("AuthDb")
                    );
                });
            });

            _client = _factory.CreateClient();
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
        // ↓↓↓ default query parameters (int page = 1, int pageSize = 5)
        [InlineData(null)]
        [InlineData("")]
        public async Task GetAll_WithQueryParameters_ReturnsOkResult(string queryParams)
        {
            // act

            var response = await _client.GetAsync("/api/categories?" + queryParams);
            // assert

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Theory]
        [InlineData("page=2&pageSize=1")]
        [InlineData("page=300&pageSize=11")]
        public async Task GetAll_WithInvalidQueryParams_ReturnsBadRequest(string queryParams)
        {
            // arrange
            var category = new Category() { Name = "test1", UrlHandle = "test-1" };
            SeedCategory(category);

            // act
            var response = await _client.GetAsync("/api/categories?" + queryParams);

            //assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetCategoryById_ForUserWithWriterRoleAndExistingCategory_ReturnsOk()
        {
            // arrange
            var existingCategory = new Category() { Name = "test1", UrlHandle = "test-1" };
            SeedCategory(existingCategory);

            // act
            var response = await _client.GetAsync("/api/categories/" + existingCategory.Id);

            // assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetCategoryById_ForUserWithWriterRoleAndNonExistingCategory_ReturnsNotFound()
        {
            // arrange
            var nonExistingCategoryId = new Guid("3b72c160-87f4-4ee2-aa80-16c6a3d51d86");

            // act
            var response = await _client.GetAsync("/api/categories/" + nonExistingCategoryId);

            // assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        // ******* //
        // Create  //
        // ******* //

        [Fact]
        public async Task CreateCategory_WithValidModel_ReturnsCreatedStatus()
        {
            // arrange
            var category = new CreateCategoryRequestDto
            {
                Name = "ValidCategory",
                UrlHandle = "valid-category"
            };

            var httpContent = category.ToJsonHttpContent();

            // act
            var response = await _client.PostAsync("/api/categories", httpContent);

            // arrange

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateCategory_WithInvalidModel_ReturnsBadRequest()
        {
            // arrange
            var category = new CreateCategoryRequestDto
            {
                Name = "ValidCategory",
                UrlHandle = "invalid!-!category"
            };
            var httpContent = category.ToJsonHttpContent();

            // act
            var response = await _client.PostAsync("/api/categories", httpContent);

            // arrange
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        // ******* //
        // Update  //
        // ******* //

        [Fact]
        public async Task UpdateCategory_WithValidModel_ReturnsNoContent()
        {
            // arrange
            var scopeFactory = _factory.Services.GetService<IServiceScopeFactory>();
            using var scope = scopeFactory.CreateScope();
            var _dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

            var existingCategory = new Category() { Name = "test1", UrlHandle = "test-1" };
            SeedCategory(existingCategory);

            var updatedCategoryDto = new UpdateCategoryRequestDto
            {
                Name = "UpdatedCategory",
                UrlHandle = "updated-category"
            };
            var httpContent = updatedCategoryDto.ToJsonHttpContent();

            // act
            var response = await _client.PutAsync(
                "/api/categories/" + existingCategory.Id,
                httpContent
            );

            // assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            var updatedCategory = await _dbContext.Categories.FindAsync(existingCategory.Id);
            updatedCategory.Name.Should().Be(updatedCategoryDto.Name);
            updatedCategory.UrlHandle.Should().Be(updatedCategoryDto.UrlHandle);
        }

        [Fact]
        public async Task UpdateCategory_WithInvalidModel_ReturnsBadRequest()
        {
            // arrange
            var existingCategory = new Category() { Name = "test1", UrlHandle = "test-1" };
            SeedCategory(existingCategory);

            var updatedCategoryDto = new UpdateCategoryRequestDto
            {
                Name = "UpdatedCategory",
                UrlHandle = "invalid!-!category"
            };
            var httpContent = updatedCategoryDto.ToJsonHttpContent();

            // act
            var response = await _client.PutAsync(
                "/api/categories/" + existingCategory.Id,
                httpContent
            );

            // assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        // ******* //
        // Delete  //
        // ******* //

        [Fact]
        public async Task Delete_ForUserWithWriterRole_ReturnsNoContent()
        {
            // arrange
            var category = new Category() { Name = "test1", UrlHandle = "test-1" };
            SeedCategory(category);

            // act
            var response = await _client.DeleteAsync("/api/categories/" + category.Id);

            // assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Delete_ForUserWithWriterRoleAndNonExistingCategory_ReturnsNotFound()
        {
            // arrange
            var nonExistingCategoryId = new Guid("3b72c160-87f4-4ee2-aa80-16c6a3d51d86");
            var existingCategory = new Category() { Name = "test1", UrlHandle = "test-1" };
            SeedCategory(existingCategory);

            // act
            var response = await _client.DeleteAsync("/api/categories/" + nonExistingCategoryId);

            // assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }
    }
}
