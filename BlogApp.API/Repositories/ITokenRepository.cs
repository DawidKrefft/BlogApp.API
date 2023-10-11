using Microsoft.AspNetCore.Identity;

namespace BlogApp.API.Repositories
{
    public interface ITokenRepository
    {
        string CreateJwtToken(IdentityUser user, List<string> roles);
    }
}
