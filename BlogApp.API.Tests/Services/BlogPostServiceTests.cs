using AutoMapper;
using BlogApp.API.Data;
using BlogApp.API.Models.Domain;
using BlogApp.API.Models.DTO;
using BlogApp.API.Services;
using BlogApp.API.Tests.InMemDatabases;
using BlogApp.API.Validations;
using FluentAssertions;
using FluentValidation;

namespace BlogApp.API.Tests.Services
{
    public class BlogPostServiceTests
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IMapper mapper;
        private readonly IValidator<CreateBlogPostRequestDto> createBlogPostValidator;
        private readonly IValidator<UpdateBlogPostRequestDto> updateBlogPostValidator;
        private readonly BlogPostService blogPostService;

        public BlogPostServiceTests()
        {
            var inMemoryContext = new InMemApplicationDbContext();
            dbContext = inMemoryContext.GetDatabaseContext().Result;
            mapper = CreateMapper();
            createBlogPostValidator = new CreateBlogPostValidator();
            updateBlogPostValidator = new UpdateBlogPostValidator();
            blogPostService = new BlogPostService(
                dbContext,
                mapper,
                createBlogPostValidator,
                updateBlogPostValidator
            );
        }

        private IMapper CreateMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<BlogPostProfile>();
            });
            return config.CreateMapper();
        }

        // ******************************************************
        // GetAllAsync(int page, int pageSize)
        // ******************************************************

        [Theory]
        [InlineData(1, 1, "Sample Blog Post 1")]
        [InlineData(1, 2, "Sample Blog Post 1", "Sample Blog Post 2")]
        [InlineData(4, 1, "Sample Blog Post 4")]
        public async Task GetAllAsync_WithValidInput_ShouldReturnBlogPostDto(
            int page,
            int pageSize,
            params string[] expectedTitles
        )
        {
            // Arrange & Act
            var result = await blogPostService.GetAllAsync(page, pageSize);
            var actualTitles = result.Items.Select(item => item.Title).ToList();

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().NotBeNull();
            result.Items.Should().AllBeOfType<BlogPostDto>();
            result.Items.Should().HaveCount(Math.Min(pageSize, 10));
            actualTitles.Should().BeEquivalentTo(expectedTitles);
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
                await blogPostService.GetAllAsync(page, pageSize);
            });
            exception.Message.Should().Be("Page or PageSize cannot be 0");
        }

        // ******************************************************
        //GetByIdAsync(Guid id)
        // ******************************************************

        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnBlogPostDto()
        {
            // Arrange
            var existingCategoryId = new Guid("1E3279E3-4810-41C8-831C-3E0A10C4EAC3");
            var expectedBlogPost = new BlogPostDto
            {
                Id = new Guid("1E3279E3-4810-41C8-831C-3E0A10C4EAC3"),
                Title = "Sample Blog Post 1",
                ShortDescription = "This is a sample blog post 1.",
                Content = "Lorem ipsum...",
                FeaturedImageUrl = "https://localhost:7055/Images/sample1.jpg",
                UrlHandle = "url-handle1",
                PublishedDate = new DateTime(2023, 1, 1),
                Author = "John Doe",
                IsVisible = true,
                Categories = new List<CategoryDto>
                {
                    new CategoryDto
                    {
                        Id = new Guid("B0385524-D562-48AC-8A39-08DBC68D59C0"),
                        Name = "Category1",
                        UrlHandle = "category-1"
                    }
                }
            };

            // Act
            var result = await blogPostService.GetByIdAsync(existingCategoryId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedBlogPost);
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ShouldThrowException()
        {
            // Get a non-existent category ID
            var nonExistentBlogPostId = new Guid("F0385524-D562-48AC-8A39-08DBC68D59C0");

            // Act and Assert
            await FluentActions
                .Invoking(async () => await blogPostService.GetByIdAsync(nonExistentBlogPostId))
                .Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Failed to retrieve a blog post by ID.");
        }

        // ******************************************************
        // CreateAsync(CreateCategoryRequestDto request)
        // ******************************************************

        [Fact]
        public async Task CreateAsync_WithValidInput_ShouldReturnCategoryDto()
        {
            // Arrange
            var validRequest = new CreateBlogPostRequestDto
            {
                Title = "Sample Blog Post 1",
                ShortDescription = "This is a sample blog post 1.",
                Content = "Lorem ipsum...",
                FeaturedImageUrl = "https://localhost:7055/Images/sample1.jpg",
                UrlHandle = "url-handle1",
                PublishedDate = new DateTime(2023, 1, 1),
                Author = "John Doe",
                IsVisible = true,
                Categories = new Guid[]
                {
                    new Guid("1B24C116-A18F-4949-9F1A-60A88B30BB67"),
                    new Guid("C5F2C59A-EAC1-44A1-8E64-524EB804A9F2")
                }
            };

            // Act
            var result = await blogPostService.CreateAsync(validRequest);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<BlogPostDto>();

            result.Categories.Should().HaveCount(2);
            result.Categories.Should().Contain(category => category.Name == "Category5");
            result.Categories.Should().Contain(category => category.Name == "Category6");
        }

        [Theory]
        [InlineData(
            "FiftyOneCharsFiftyOneCharsFiftyOneCharsFiftyOneCharsFiftyOneCharsFiftyOneChars",
            "This is a sample blog post 1.",
            "Lorem ipsum...",
            "https://localhost:7055/Images/sample1.jpg",
            "url-handle1!",
            "2022-01-01",
            "",
            true,
            new[] { "B0385524-D562-48AC-8A39-08DBC68D59C0" },
            "The length of 'Title' must be 30 characters or fewer. You entered 78 characters., URL handle can only contain letters, numbers, and hyphens., 'Author' must not be empty."
        )]
        [InlineData(
            "ValidTitle",
            "Short description.",
            "Content of the blog post.",
            "https://localhost:7055/Images/sample2.jpg",
            "valid-url-handle",
            "2023-05-15",
            "Jane Doe",
            true,
            new[] { "B0385524-D562-48AC-8A39-08DBC68D5900" },
            "Category not found."
        )]
        public async Task CreateAsync_WithInvalidInput_ShouldThrowValidationException(
            string title,
            string shortDescription,
            string content,
            string featuredImageUrl,
            string urlHandle,
            string publishedDate,
            string author,
            bool isVisible,
            string[] categoryIds,
            string expectedErrorMessage
        )
        {
            // Arrange
            var invalidRequest = new CreateBlogPostRequestDto
            {
                Title = title,
                ShortDescription = shortDescription,
                Content = content,
                FeaturedImageUrl = featuredImageUrl,
                UrlHandle = urlHandle,
                PublishedDate = DateTime.Parse(publishedDate),
                Author = author,
                IsVisible = isVisible,
                Categories = categoryIds?.Select(id => new Guid(id)).ToArray()
            };

            // Act and Assert
            await FluentActions
                .Invoking(async () => await blogPostService.CreateAsync(invalidRequest))
                .Should()
                .ThrowAsync<Exception>()
                .Where(
                    exception =>
                        (exception is ValidationException)
                        || (exception is InvalidOperationException)
                )
                .WithMessage(expectedErrorMessage);
        }
    }
}
