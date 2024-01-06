using BlogApp.API.Models.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.API.Services.Interfaces
{
    public interface IAuthService
    {
        string CreateJwtToken(IdentityUser user, List<string> roles);
        Task<LoginResponseDto> Login(LoginRequestDto request);
        Task<IdentityUser> RegisterAsync(RegisterRequestDto request);
        Task<bool> DeleteAccountAsync(Guid userId);
    }
}
