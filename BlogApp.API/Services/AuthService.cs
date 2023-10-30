using AutoMapper;
using BlogApp.API.Models.DTO;
using BlogApp.API.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BlogApp.API.Services
{
    public class AuthService : IAuthRepository
    {
        private readonly IConfiguration configuration;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IMapper mapper;
        private readonly IValidator<RegisterRequestDto> registerValidator;

        public AuthService(
            IConfiguration configuration,
            UserManager<IdentityUser> userManager,
            IMapper mapper,
            IValidator<RegisterRequestDto> registerValidator
        )
        {
            this.configuration = configuration;
            this.userManager = userManager;
            this.mapper = mapper;
            this.registerValidator = registerValidator;
        }

        public async Task<LoginResponseDto> Login(LoginRequestDto request)
        {
            var identityUser = await userManager.FindByEmailAsync(request.Email);

            if (identityUser == null || !await IsPasswordValid(identityUser, request.Password))
            {
                throw new InvalidOperationException("Failed to login.");
            }

            var roles = await userManager.GetRolesAsync(identityUser);
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

            if (await IsEmailInUse(request.Email))
            {
                throw new ValidationException("Email is already in use.");
            }

            var user = mapper.Map<IdentityUser>(request);
            user.UserName = user.Email?.Trim(); // Set UserName from Email

            var identityResult = await userManager.CreateAsync(user, request.Password);

            if (!identityResult.Succeeded)
            {
                HandleIdentityErrors(identityResult);
                return null;
            }

            if (!await AddUserRole(user, "Reader"))
            {
                return null;
            }

            return user;
        }

        public async Task<bool> DeleteAccountAsync(Guid userId)
        {
            var adminUserId = configuration["UserIds:Admin"];
            var user = await userManager.FindByIdAsync(userId.ToString());

            if (user == null)
            {
                return false;
            }

            if (user.Id == adminUserId)
            {
                throw new InvalidOperationException("Cannot delete the admin user.");
            }

            var result = await userManager.DeleteAsync(user);
            return result.Succeeded;
        }

        public string CreateJwtToken(IdentityUser user, List<string> roles)
        {
            // Create Claims
            var claims = new List<Claim> { new Claim(ClaimTypes.Email, user.Email) };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            // JWT Security Token Parameters
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

        private async Task<bool> IsPasswordValid(IdentityUser user, string password)
        {
            return await userManager.CheckPasswordAsync(user, password);
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

        private async Task<bool> AddUserRole(IdentityUser user, string roleName)
        {
            var identityResult = await userManager.AddToRoleAsync(user, roleName);
            if (!identityResult.Succeeded)
            {
                HandleIdentityErrors(identityResult);
                return false;
            }
            return true;
        }

        private async Task<bool> IsEmailInUse(string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            return user != null;
        }
    }
}
