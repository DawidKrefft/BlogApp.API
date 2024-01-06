using BlogApp.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace BlogApp.API.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly UserManager<IdentityUser> userManager;

        public AuthRepository(IConfiguration configuration, UserManager<IdentityUser> userManager)
        {
            this.userManager = userManager;
        }

        public async Task<IdentityUser> FindByEmailAsync(string email)
        {
            return await userManager.FindByEmailAsync(email);
        }

        public async Task<IList<string>> GetRolesAsync(IdentityUser user)
        {
            return await userManager.GetRolesAsync(user);
        }

        public async Task<IdentityUser> FindByIdAsync(Guid userId)
        {
            return await userManager.FindByIdAsync(userId.ToString());
        }

        public async Task<IdentityResult> CreateUserAsync(IdentityUser user, string password)
        {
            return await userManager.CreateAsync(user, password);
        }

        public async Task<bool> DeleteUserAsync(IdentityUser user)
        {
            return (await userManager.DeleteAsync(user)).Succeeded;
        }

        public async Task<bool> AddToRoleAsync(IdentityUser user, string roleName)
        {
            return (await userManager.AddToRoleAsync(user, roleName)).Succeeded;
        }

        public async Task<bool> IsPasswordValid(IdentityUser user, string password)
        {
            return await userManager.CheckPasswordAsync(user, password);
        }

        public async Task<bool> IsEmailInUse(string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            return user != null;
        }
    }
}
