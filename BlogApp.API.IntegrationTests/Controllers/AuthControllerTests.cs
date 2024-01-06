using BlogApp.API.IntegrationTests.Helpers;
using BlogApp.API.Models.DTO;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using BlogApp.API.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using FluentValidation;
using BlogApp.API.Services.Interfaces;

namespace BlogApp.API.IntegrationTests.Controllers
{
    public class AuthControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private HttpClient _client;

        private Mock<IAuthService> _authServiceMock = new Mock<IAuthService>();

        public AuthControllerTests(WebApplicationFactory<Program> factory)
        {
            _client = factory
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        var dbContextOptions = services.SingleOrDefault(
                            service =>
                                service.ServiceType == typeof(DbContextOptions<AuthDbContext>)
                        );

                        services.Remove(dbContextOptions);

                        services.AddSingleton<IAuthService>(_authServiceMock.Object);

                        services.AddDbContext<AuthDbContext>(
                            options => options.UseInMemoryDatabase("AuthDb")
                        );
                    });
                })
                .CreateClient();
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsOk()
        {
            // Arrange
            _authServiceMock
                .Setup(e => e.Login(It.IsAny<LoginRequestDto>()))
                .ReturnsAsync(new LoginResponseDto());

            var loginRequest = new LoginRequestDto()
            {
                Email = "correctUser@test.com",
                Password = "Correct@123"
            };

            var httpContent = new StringContent(
                JsonConvert.SerializeObject(loginRequest),
                Encoding.UTF8,
                "application/json"
            );

            // Act
            var response = await _client.PostAsync("/api/auth/login", httpContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsInternalServerError()
        {
            // Arrange
            _authServiceMock
                .Setup(e => e.Login(It.IsAny<LoginRequestDto>()))
                .ReturnsAsync(
                    (LoginRequestDto incorrectRequest) =>
                    {
                        if (
                            incorrectRequest.Email == "admin@test.com"
                            && incorrectRequest.Password == "IncorrectPassword"
                        )
                        {
                            throw new InvalidOperationException("Failed to login.");
                        }

                        return new LoginResponseDto();
                    }
                );

            var loginRequest = new LoginRequestDto()
            {
                Email = "admin@test.com",
                Password = "IncorrectPassword"
            };

            var httpContent = new StringContent(
                JsonConvert.SerializeObject(loginRequest),
                Encoding.UTF8,
                "application/json"
            );

            // Act
            var response = await _client.PostAsync("/api/auth/login", httpContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task Register_WithValidRequest_ReturnsOk()
        {
            // Arrange
            var registerRequest = new RegisterRequestDto
            {
                Email = "newUser",
                Password = "password123"
            };

            var httpContent = registerRequest.ToJsonHttpContent();

            // Act
            var response = await _client.PostAsync("/api/auth/register", httpContent);

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task RegisterUser_ForInvalidModel_ReturnsBadRequest()
        {
            // arrange
            _authServiceMock
                .Setup(e => e.RegisterAsync(It.IsAny<RegisterRequestDto>()))
                .ThrowsAsync(new ValidationException("Password is required"));

            var registerUser = new RegisterRequestDto() { Email = "newUser", Password = "" };

            var httpContent = registerUser.ToJsonHttpContent();

            // act
            var response = await _client.PostAsync("/api/auth/register", httpContent);

            // assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }
    }
}
