using BlogApp.API.Repositories.Implementation;
using BlogApp.API.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.API.Data
{
    public static class DataExtensions
    {
        public static async Task InitializeDBAsync(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.MigrateAsync();

            var authDbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            await authDbContext.Database.MigrateAsync();

            var logger = serviceProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("DB Initializer");
            logger.LogInformation(5, "The databases are ready");
        }

        public static IServiceCollection AddRepositories(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            var connString = configuration.GetConnectionString("BlogAppConnectionString");
            services
                .AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connString))
                .AddDbContext<AuthDbContext>(options => options.UseSqlServer(connString))
                .AddScoped<ICategoryRepository, CategoryRepository>()
                .AddScoped<IBlogPostRepository, BlogPostRepository>()
                .AddScoped<IImageRepository, ImageRepository>()
                .AddScoped<ITokenRepository, TokenRepository>();

            return services;
        }
    }
}
