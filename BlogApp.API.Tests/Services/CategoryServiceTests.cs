using AutoMapper;
using BlogApp.API.Data;
using BlogApp.API.Models.DTO;
using BlogApp.API.Models.Profiles;
using BlogApp.API.Services;
using FluentAssertions;
using FluentValidation;
using BlogApp.API.Tests.InMemDatabases;
using BlogApp.API.Validations;

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

        //[Fact]
        //public async Task GetAllAsync_ShouldReturnCategories()
        //{
        //    var dbContext = await GetDatabaseContext();

        //    // Act
        //    var result = await categoryService.GetAllAsync();

        //    // Assert
        //    result.Should().NotBeEmpty();
        //    result.Should().BeAssignableTo<IEnumerable<Category>>();
        //}

        //[Fact]
        //public async Task GetAllAsync_ShouldReturnCategoryDtoList()
        //{
        //    // Arrange
        //    var dbContext = await GetDatabaseContext();

        //    // Act
        //    var result = await categoryService.GetAllAsync();

        //    // Assert
        //    result.Should().NotBeNull(); // Ensure the result is not null
        //}



        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnCategoryDtoV2()
        {
            // Arrange

            var existingCategoryId = new Guid("B0385524-D562-48AC-8A39-08DBC68D59C0");

            // Act

            var result = await categoryService.GetByIdAsync(existingCategoryId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<CategoryDto>();
            result.Name.Should().Be("Category 1");
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

        // CREATE

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
            // Additional assertions if needed, e.g., checking that the category is persisted in the database.
        }

        // Faking Validations example
        //[Fact]
        //public async Task CreateAsync_WithInvalidInput_ShouldThrowValidationException()
        //{
        //    // Arrange
        //    var invalidRequest = new CreateCategoryRequestDto
        //    {
        //        Name = "",  // Invalid name
        //        UrlHandle = "valid-category"
        //    };

        //    // Mock the validator to return validation errors
        //    A.CallTo(() => createCategoryValidator.ValidateAsync(A<CreateCategoryRequestDto>._))
        //        .Returns(new FluentValidation.Results.ValidationResult(new List<ValidationFailure>
        //        {
        //    new ValidationFailure("Name", "Name is required")
        //        }));

        //    // Act and Assert
        //    await FluentActions
        //        .Invoking(async () => await categoryService.CreateAsync(invalidRequest))
        //        .Should()
        //        .ThrowAsync<ValidationException>()
        //        .WithMessage("Name is required");
        //}


        [Fact]
        public async Task CreateAsync_WithInvalidInput_ShouldThrowValidationException()
        {
            // Arrange
            var invalidRequest = new CreateCategoryRequestDto { Name = "", UrlHandle = "" };

            // Act and Assert
            await FluentActions
                .Invoking(async () => await categoryService.CreateAsync(invalidRequest))
                .Should()
                .ThrowAsync<ValidationException>()
                .WithMessage(
                    "'Name' must not be empty., Name can only contain letters, numbers, and hyphens., 'Url Handle' must not be empty., URL handle can only contain lowercase letters, numbers, and hyphens."
                );
        }

        [Fact]
        public async Task CreateAsync_WithDatabaseException_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var validRequest = new CreateCategoryRequestDto
            {
                Name = "Valid Category",
                UrlHandle = "valid-category"
            };
            // Simulate a database failure by providing a faulty DbContext, e.g., one that always throws an exception on SaveChangesAsync.

            // Act and Assert
            await FluentActions
                .Invoking(async () => await categoryService.CreateAsync(validRequest))
                .Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Failed to create the category.");
        }

        [Fact]
        public async Task CreateAsync_WithSpecificValidationMessages_ShouldThrowValidationException()
        {
            // Arrange
            var invalidRequest = new CreateCategoryRequestDto
            {
                Name = "",
                UrlHandle = "invalid-category"
            };

            // Act and Assert
            await FluentActions
                .Invoking(async () => await categoryService.CreateAsync(invalidRequest))
                .Should()
                .ThrowAsync<ValidationException>()
                .WithMessage("*expected validation error message*"); // Replace with the expected validation error message.
        }
    }
}
