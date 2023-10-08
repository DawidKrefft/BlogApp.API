using BlogApp.API.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace BlogApp.API.Authorization
{
    public static class AuthorizationExtensions
    {
        public static void AddBlogAppIdentity(this IServiceCollection services)
        {
            services
                .AddIdentityCore<IdentityUser>()
                .AddRoles<IdentityRole>()
                .AddTokenProvider<DataProtectorTokenProvider<IdentityUser>>("BlogApp")
                .AddEntityFrameworkStores<AuthDbContext>()
                .AddDefaultTokenProviders();
        }

        public static void ConfigureIdentityOptions(this IServiceCollection services)
        {
            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;
            });
        }

        public static void AddJwtAuthentication(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        AuthenticationType = "Jwt",
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = configuration["Jwt:Issuer"],
                        ValidAudience = configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            System.Text.Encoding.UTF8.GetBytes(configuration["Jwt:Key"])
                        )
                    };
                });
        }
    }
}
