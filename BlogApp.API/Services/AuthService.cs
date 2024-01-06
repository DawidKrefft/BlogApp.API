using AutoMapper;
using BlogApp.API.Exceptions;
using BlogApp.API.Models.DTO;
using BlogApp.API.Repositories.Interfaces;
using BlogApp.API.Services.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BlogApp.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration configuration;
        private readonly IAuthRepository authRepository;
        private readonly IMapper mapper;
        private readonly IValidator<RegisterRequestDto> registerValidator;

        public AuthService(
            IConfiguration configuration,
            IAuthRepository authRepository,
            IMapper mapper,
            IValidator<RegisterRequestDto> registerValidator
        )
        {
            this.configuration = configuration;
            this.authRepository = authRepository;
            this.mapper = mapper;
            this.registerValidator = registerValidator;
        }

        public async Task<LoginResponseDto> Login(LoginRequestDto request)
        {
            var identityUser = await authRepository.FindByEmailAsync(request.Email);

            if (
                identityUser == null
                || !await authRepository.IsPasswordValid(identityUser, request.Password)
            )
            {
                throw new InvalidOperationException("Failed to login.");
            }

            var roles = await authRepository.GetRolesAsync(identityUser);
            var jwtToken = CreateJwtToken(identityUser, roles.ToList());

            var response = mapper.Map<LoginResponseDto>(identityUser);
            response.Roles = roles.ToList();
            response.Token = jwtToken;

            return response;
        }

        public async Task<IdentityUser> RegisterAsync(RegisterRequestDto request)
        {
            var validation = await registerValidator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                var validationErrors = string.Join(
                    ", ",
                    validation.Errors.Select(error => error.ErrorMessage)
                );
                throw new ValidationException(validationErrors);
            }

            if (await authRepository.IsEmailInUse(request.Email))
            {
                throw new ValidationException("Email is already in use.");
            }

            var user = mapper.Map<IdentityUser>(request);
            user.UserName = user.Email?.Trim(); // Set UserName from Email

            var identityResult = await authRepository.CreateUserAsync(user, request.Password);

            if (!identityResult.Succeeded)
            {
                HandleIdentityErrors(identityResult);
                return null;
            }

            if (!await authRepository.AddToRoleAsync(user, "Reader"))
            {
                return null;
            }

            return user;
        }

        public async Task<bool> DeleteAccountAsync(Guid userId)
        {
            var adminUserId = configuration["UserIds:Admin"];
            var user = await authRepository.FindByIdAsync(userId);

            return user == null
                ? false
                : user.Id == adminUserId
                    ? throw new BadRequestException("Cannot delete the admin user.")
                    : await authRepository.DeleteUserAsync(user);
        }

        public string CreateJwtToken(IdentityUser user, List<string> roles)
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.Email, user.Email) };
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: configuration["Jwt:Issuer"],
                audience: configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    Convert.ToDouble(configuration["Jwt:TokenExpirationMinutes"])
                ),
                signingCredentials: credentials
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private IEnumerable<string> HandleIdentityErrors(IdentityResult result)
        {
            var errors = new List<string>();

            foreach (var error in result.Errors)
            {
                errors.Add(error.Description);
            }
            return errors;
        }
    }
}
