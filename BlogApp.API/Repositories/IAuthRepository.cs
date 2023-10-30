using BlogApp.API.Models.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.API.Repositories
{
    public interface IAuthRepository
    {
        string CreateJwtToken(IdentityUser user, List<string> roles);
        Task<LoginResponseDto> Login(LoginRequestDto request);
        Task<IdentityUser> RegisterAsync(RegisterRequestDto request);
        Task<bool> DeleteAccountAsync(Guid userId);
    }
}
