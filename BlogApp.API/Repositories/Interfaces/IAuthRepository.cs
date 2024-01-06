using Microsoft.AspNetCore.Identity;

namespace BlogApp.API.Repositories.Interfaces
{
    public interface IAuthRepository
    {
        Task<IdentityUser> FindByEmailAsync(string email);
        Task<IList<string>> GetRolesAsync(IdentityUser user);
        Task<IdentityUser> FindByIdAsync(Guid userId);
        Task<IdentityResult> CreateUserAsync(IdentityUser user, string password);
        Task<bool> DeleteUserAsync(IdentityUser user);
        Task<bool> AddToRoleAsync(IdentityUser user, string roleName);
        Task<bool> IsPasswordValid(IdentityUser user, string password);
        Task<bool> IsEmailInUse(string email);
    }
}
