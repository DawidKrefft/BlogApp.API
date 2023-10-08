using BlogApp.API.Models.DTO;
using BlogApp.API.Repositories.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly ITokenRepository tokenRepository;

        public AuthController(
            UserManager<IdentityUser> userManager,
            ITokenRepository tokenRepository
        )
        {
            this.userManager = userManager;
            this.tokenRepository = tokenRepository;
        }

        // POST: {apiBaseUrl}/api/auth/login
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var identityUser = await userManager.FindByEmailAsync(request.Email);

            if (identityUser != null)
            {
                if (await IsPasswordValid(identityUser, request.Password))
                {
                    var roles = await userManager.GetRolesAsync(identityUser);
                    var jwtToken = tokenRepository.CreateJwtToken(identityUser, roles.ToList());

                    var response = new LoginResponseDto()
                    {
                        Email = request.Email,
                        Roles = roles.ToList(),
                        Token = jwtToken
                    };

                    return Ok(response);
                }
            }

            ModelState.AddModelError("", "Email or Password Incorrect");
            return ValidationProblem(ModelState);
        }

        // POST: {apiBaseUrl}/api/auth/register
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            var user = new IdentityUser
            {
                UserName = request.Email?.Trim(),
                Email = request.Email?.Trim()
            };

            var identityResult = await userManager.CreateAsync(user, request.Password);

            if (!identityResult.Succeeded)
            {
                HandleIdentityErrors(identityResult);
                return ValidationProblem(ModelState);
            }

            if (!await AddUserRole(user, "Reader"))
            {
                ModelState.AddModelError("", "Failed to assign role to the user.");
                return ValidationProblem(ModelState);
            }

            return Ok();
        }

        private async Task<bool> IsPasswordValid(IdentityUser user, string password)
        {
            return await userManager.CheckPasswordAsync(user, password);
        }

        private void HandleIdentityErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
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
    }
}
