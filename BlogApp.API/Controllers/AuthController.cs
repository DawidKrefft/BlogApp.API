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

        /// <summary>
        /// Handles user login and returns a bearer token with login data.
        /// </summary>
        /// <param name="request">The login request DTO containing user credentials.</param>
        /// <returns>Returns a bearer token with login data.</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var loginResponse = await authRepository.Login(request);
            return Ok(loginResponse);
        }

        /// <summary>
        /// Registers a new user account.
        /// </summary>
        /// <param name="request">The registration request DTO containing user information.</param>
        /// <returns>Returns a message indicating the success of the account creation.</returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            await authRepository.RegisterAsync(request);
            return Ok(new { message = "Account created successfully." });
        }

        /// <summary>
        /// Deletes a user account with the specified ID (requires "Writer" role).
        /// </summary>
        /// <param name="id">The unique identifier of the user account to be deleted.</param>
        /// <returns>Returns NoContent if the account is successfully deleted; otherwise, NotFound.</returns>
        [Authorize(Roles = "Writer")]
        [HttpDelete("{id:Guid}")]
        public async Task<IActionResult> DeleteAccount([FromRoute] Guid id)
        {
            var result = await authRepository.DeleteAccountAsync(id);
            return result ? NoContent() : NotFound();
        }
    }
}
