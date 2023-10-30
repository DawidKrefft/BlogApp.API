using BlogApp.API.Models.DTO;
using BlogApp.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository authRepository;

        public AuthController(IAuthRepository authRepository)
        {
            this.authRepository = authRepository;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var loginResponse = await authRepository.Login(request);
            return Ok(loginResponse);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            await authRepository.RegisterAsync(request);
            return Ok("Account created successfully.");
        }

        [Authorize(Roles = "Writer")]
        [HttpDelete("{id:Guid}")]
        public async Task<IActionResult> DeleteAccount([FromRoute] Guid id)
        {
            var result = await authRepository.DeleteAccountAsync(id);
            return result ? NoContent() : NotFound();
        }
    }
}
