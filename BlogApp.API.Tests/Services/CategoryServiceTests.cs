using AutoMapper;
using BlogApp.API.Data;
using BlogApp.API.Models.DTO;
using BlogApp.API.Models.Profiles;
using BlogApp.API.Services;
using FluentAssertions;
using FluentValidation;
using BlogApp.API.Tests.InMemDatabases;
using BlogApp.API.Validations;
using FakeItEasy;

namespace BlogApp.API.Tests.Services
{
    public class CategoryServiceTests
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IMapper mapper;
        private readonly IValidator<CreateCategoryRequestDto> createCategoryValidator;
        private readonly IValidator<UpdateCategoryRequestDto> updateCategoryValidator;
        private readonly CategoryService categoryService;

        public CategoryServiceTests()
        {
            var inMemoryContext = new InMemApplicationDbContext();
            dbContext = inMemoryContext.GetDatabaseContext().Result;
            mapper = CreateMapper();
            createCategoryValidator = new CreateCategoryValidator();
            updateCategoryValidator = new UpdateCategoryValidator();
            categoryService = new CategoryService(
                dbContext,
                mapper,
                createCategoryValidator,
                updateCategoryValidator
            );
        }

        private IMapper CreateMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<CategoryProfile>();
            });
            return config.CreateMapper();
        }

        // ******************************************************
        // GetAllAsync(int page, int pageSize)
        // ******************************************************

        [Theory]
        [InlineData(1, 1, "Category1")]
        [InlineData(1, 3, "Category1", "Category2", "Category3")]
        [InlineData(2, 2, "Category3", "Category4")]
        public async Task GetAllAsync_WithValidInput_ShouldReturnCategoryDto(
            int page,
            int pageSize,
            params string[] expectedNames
        )
        {
            // Arrange & Act

            var result = await categoryService.GetAllAsync(page, pageSize);
            var actualNames = result.Items.Select(item => item.Name).ToList();

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().NotBeNull();
            result.Items.Should().AllBeOfType<CategoryDto>();
            result.Items.Should().HaveCount(Math.Min(pageSize, 50));
            actualNames.Should().BeEquivalentTo(expectedNames);
        }

        [Theory]
        [InlineData(0, 5)]
        [InlineData(1, 0)]
        [InlineData(0, 0)]
        public async Task GetAllAsync_WithInvalidInput_ShouldReturnException(int page, int pageSize)
        {
            // Act and Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await categoryService.GetAllAsync(page, pageSize);
            });
            exception.Message.Should().Be("Page or PageSize cannot be 0");
        }

        // ******************************************************
        //GetByIdAsync(Guid id)
        // ******************************************************

        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnCategoryDto()
        {
            // Arrange

            var existingCategoryId = new Guid("B0385524-D562-48AC-8A39-08DBC68D59C0");

            // Act

            var result = await categoryService.GetByIdAsync(existingCategoryId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<CategoryDto>();
            result.Name.Should().Be("Category1");
            result.UrlHandle.Should().Be("category-1");
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ShouldThrowException()
        {
            // Get a non-existent category ID
            var nonExistentCategoryId = new Guid("F0385524-D562-48AC-8A39-08DBC68D59C0");

            // Act and Assert
            await FluentActions
                .Invoking(async () => await categoryService.GetByIdAsync(nonExistentCategoryId))
                .Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Failed to retrieve a category by ID.");
        }

        // ******************************************************
        // CreateAsync(CreateCategoryRequestDto request)
        // ******************************************************

        [Fact]
        public async Task CreateAsync_WithValidInput_ShouldReturnCategoryDto()
        {
            // Arrange
            var validRequest = new CreateCategoryRequestDto
            {
                Name = "ValidCategory",
                UrlHandle = "valid-category"
            };

            // Act
            var result = await categoryService.CreateAsync(validRequest);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<CategoryDto>();
            result.Name.Should().Be("ValidCategory");
            result.UrlHandle.Should().Be("valid-category");
        }

        [Theory]
        [InlineData(
            "",
            "",
            "'Name' must not be empty., Name can only contain letters, numbers, and hyphens., 'Url Handle' must not be empty., URL handle can only contain lowercase letters, numbers, and hyphens."
        )]
        [InlineData(
            "ValidName",
            "",
            "'Url Handle' must not be empty., URL handle can only contain lowercase letters, numbers, and hyphens."
        )]
        [InlineData(
            "",
            "validurl",
            "'Name' must not be empty., Name can only contain letters, numbers, and hyphens."
        )]
        [InlineData("name!", "validurl", "Name can only contain letters, numbers, and hyphens.")]
        [InlineData(
            "name",
            "validurl!",
            "URL handle can only contain lowercase letters, numbers, and hyphens."
        )]
        [InlineData(
            "FiftyOneCharsFiftyOneCharsFiftyOneCharsFiftyOneCharsFiftyOneCharsFiftyOneChars",
            "FiftyOneCharsFiftyOneCharsFiftyOneCharsFiftyOneCharsFiftyOneCharsFiftyOneChars",
            "Name is required and must be less than 50 characters., URL handle is required and must be less than 50 characters., URL handle can only contain lowercase letters, numbers, and hyphens."
        )]
        public async Task CreateAsync_WithInvalidInput_ShouldThrowValidationException(
            string name,
            string urlHandle,
            string expectedErrorMessage
        )
        {
            // Arrange
            var invalidRequest = new CreateCategoryRequestDto
            {
                Name = name,
                UrlHandle = urlHandle
            };

            // Act and Assert
            await FluentActions
                .Invoking(async () => await categoryService.CreateAsync(invalidRequest))
                .Should()
                .ThrowAsync<ValidationException>()
                .WithMessage(expectedErrorMessage);
        }

        [Fact]
        public async Task CreateAsync_WithDatabaseException_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var createCategoryRequest = new CreateCategoryRequestDto
            {
                Name = "name",
                UrlHandle = "url-handle"
            };

            // Act
            async Task Act()
            {
                // if sth else goes wrong other than invalid inputs
                var service = new CategoryService(
                    dbContext,
                    A.Fake<IMapper>(),
                    new CreateCategoryValidator(),
                    A.Fake<IValidator<UpdateCategoryRequestDto>>()
                );
                await service.CreateAsync(createCategoryRequest);
            }

            // Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(Act);
            exception.Message.Should().Be("Failed to create the category.");
        }

        // ******************************************************
        // UpdateAsync(Guid id, UpdateCategoryRequestDto request)
        // ******************************************************


        [Theory]
        [InlineData(
            "B0385524-D562-48AC-8A39-08DBC68D59C0",
            "Category1updated",
            "category-1updated"
        )]
        [InlineData(
            "A7DFAABD-2E85-4F9E-B9DD-6C784C84B3A2",
            "Category3updated",
            "category-3updated"
        )]
        [InlineData(
            "1B24C116-A18F-4949-9F1A-60A88B30BB67",
            "Category5updated",
            "category-5updated"
        )]
        public async Task UpdateAsync_WithValidInput_ShouldUpdateCategory(
            Guid id,
            string updatedName,
            string updatedUrl
        )
        {
            // Arrange
            var existingCategory = dbContext.Categories.FirstOrDefault(c => c.Id == id);
            var request = new UpdateCategoryRequestDto
            {
                Name = updatedName,
                UrlHandle = updatedUrl
            };

            // Act
            var result = await categoryService.UpdateAsync(existingCategory.Id, request);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(updatedName);
            result.UrlHandle.Should().Be(updatedUrl);
        }

        [Fact]
        public async Task UpdateAsync_WithNonExistentCategory_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var nonExistentCategoryId = new Guid("B0385524-D562-48AC-8A39-08DBC0000000");
            var request = new UpdateCategoryRequestDto
            {
                Name = "updatedName",
                UrlHandle = "updated-url"
            };

            // Act
            Func<Task> act = async () =>
                await categoryService.UpdateAsync(nonExistentCategoryId, request);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Category not found.");
        }

        [Theory]
        [InlineData("InvalidCategoryName")]
        [InlineData("")]
        public async Task UpdateAsync_WithInvalidInput_ShouldThrowValidationException(
            string categoryName
        )
        {
            // Arrange
            var existingCategory = dbContext.Categories.FirstOrDefault();
            var request = new UpdateCategoryRequestDto { Name = categoryName };

            // Act and Assert
            await Assert.ThrowsAsync<ValidationException>(async () =>
            {
                await categoryService.UpdateAsync(existingCategory.Id, request);
            });
        }

        // ******************************************************
        // DeleteAsync(Guid id)
        // ******************************************************

        [Fact]
        public async Task DeleteAsync_WithValidCategoryId_ShouldReturnTrue()
        {
            // Arrange
            var existingCategory = dbContext.Categories.FirstOrDefault();

            // Act
            var result = await categoryService.DeleteAsync(existingCategory.Id);

            // Assert
            result.Should().BeTrue();
            dbContext.Categories.Should().NotContain(existingCategory);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistentCategoryId_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var nonExistentCategoryId = new Guid("B0385524-D562-48AC-8A39-08DBC0000000");

            // Act
            Func<Task> act = async () => await categoryService.DeleteAsync(nonExistentCategoryId);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Category not found.");
        }
    }
}
