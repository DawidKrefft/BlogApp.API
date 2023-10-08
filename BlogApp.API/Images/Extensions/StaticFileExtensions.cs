using Microsoft.Extensions.FileProviders;

namespace BlogApp.API.Images.Extensions
{
    public static class StaticFileExtensions
    {
        public static void UseBlogAppStaticFiles(this IApplicationBuilder app)
        {
            app.UseStaticFiles(
                new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(
                        Path.Combine(Directory.GetCurrentDirectory(), "Images")
                    ),
                    RequestPath = "/Images"
                }
            );
        }
    }
}
