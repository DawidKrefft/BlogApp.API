using BlogApp.API.Data;
using BlogApp.API.Models.Domain;
using BlogApp.API.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.API.Repositories.Implementation
{
    public class ImageRepository : IImageRepository
    {
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ApplicationDbContext dbContext;

        public ImageRepository(
            IWebHostEnvironment webHostEnvironment,
            IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext dbContext
        )
        {
            this.webHostEnvironment = webHostEnvironment;
            this.httpContextAccessor = httpContextAccessor;
            this.dbContext = dbContext;
        }

        public async Task<IEnumerable<BlogImage>> GetAll()
        {
            return await dbContext.BlogImages.ToListAsync();
        }

        public async Task<BlogImage> Upload(IFormFile file, BlogImage blogImage)
        {
            // 1) Upload the Image to API/Images
            var localPath = Path.Combine(
                webHostEnvironment.ContentRootPath,
                "Images",
                $"{blogImage.FileName}{blogImage.FileExtension}"
            );

            using var stream = new FileStream(localPath, FileMode.Create);
            await file.CopyToAsync(stream);
            // 2) Update the database

            var httpRequest = httpContextAccessor.HttpContext.Request;
            var urlPath =
                $"{httpRequest.Scheme}://{httpRequest.Host}{httpRequest.PathBase}/Images/{blogImage.FileName}{blogImage.FileExtension}";

            blogImage.Url = urlPath;

            await dbContext.BlogImages.AddAsync(blogImage);
            await dbContext.SaveChangesAsync();

            return blogImage;
        }

        public async Task<BlogImage?> DeleteAsync(Guid id)
        {
            var existingImage = await dbContext.BlogImages.FirstOrDefaultAsync(c => c.Id == id);

            if (existingImage is null)
            {
                return null;
            }

            // Retrieve the file path from the entity
            var imagePath = Path.Combine(
                webHostEnvironment.ContentRootPath,
                "Images",
                $"{existingImage.FileName}{existingImage.FileExtension}"
            );

            dbContext.BlogImages.Remove(existingImage);

            try
            {
                await dbContext.SaveChangesAsync();

                File.Delete(imagePath);
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync($"{ex}");
            }

            return existingImage;
        }
    }
}
