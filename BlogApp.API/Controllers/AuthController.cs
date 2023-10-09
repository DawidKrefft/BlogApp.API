using AutoMapper;
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
        private readonly IMapper mapper;

        public AuthController(
            UserManager<IdentityUser> userManager,
            ITokenRepository tokenRepository,
            IMapper mapper
        )
        {
            this.userManager = userManager;
            this.tokenRepository = tokenRepository;
            this.mapper = mapper;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var identityUser = await userManager.FindByEmailAsync(request.Email);

            if (identityUser != null)
            {
                if (await IsPasswordValid(identityUser, request.Password))
                {
                    var roles = await userManager.GetRolesAsync(identityUser);
                    var jwtToken = tokenRepository.CreateJwtToken(identityUser, roles.ToList());

                    var response = mapper.Map<LoginResponseDto>(identityUser);
                    response.Roles = roles.ToList();
                    response.Token = jwtToken;

                    return Ok(response);
                }
            }

            ModelState.AddModelError("", "Email or Password Incorrect");
            return ValidationProblem(ModelState);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            var user = mapper.Map<IdentityUser>(request);
            user.UserName = user.Email?.Trim(); // Set UserName from Email

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
