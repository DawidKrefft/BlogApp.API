using AutoMapper;
using Azure;
using Azure.Core;
using BlogApp.API.Data;
using BlogApp.API.Models.Domain;
using BlogApp.API.Models.DTO;
using BlogApp.API.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using static System.Net.Mime.MediaTypeNames;

namespace BlogApp.API.Services
{
    public class ImageService : IImageRepository
    {
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IMapper mapper;
        private readonly ApplicationDbContext dbContext;
        private readonly IConfiguration configuration;

        public ImageService(
            IWebHostEnvironment webHostEnvironment,
            IHttpContextAccessor httpContextAccessor,
            IMapper mapper,
            ApplicationDbContext dbContext,
            IConfiguration configuration
        )
        {
            this.webHostEnvironment = webHostEnvironment;
            this.httpContextAccessor = httpContextAccessor;
            this.mapper = mapper;
            this.dbContext = dbContext;
            this.configuration = configuration;
        }

        public async Task<IEnumerable<BlogImageDto>> GetAll()
        {
            var images = await dbContext.BlogImages.ToListAsync();
            return mapper.Map<List<BlogImageDto>>(images);
        }

        public async Task<BlogImageDto> Upload([FromForm] ImageUploadRequestDto request)
        {
            ValidateFileUpload(request);

            // File upload
            var blogImage = new BlogImage
            {
                FileExtension = Path.GetExtension(request.File.FileName).ToLower(),
                FileName = request.FileName,
                Title = request.Title,
                DateCreated = DateTime.Now,
            };

            // Upload the Image to API/Images
            var localPath = Path.Combine(
                webHostEnvironment.ContentRootPath,
                "Images",
                $"{blogImage.FileName}{blogImage.FileExtension}"
            );
            using var stream = new FileStream(localPath, FileMode.Create);
            await request.File.CopyToAsync(stream);

            // Update the database
            var httpRequest = httpContextAccessor.HttpContext.Request;
            var urlPath =
                $"{httpRequest.Scheme}://{httpRequest.Host}{httpRequest.PathBase}/Images/{blogImage.FileName}{blogImage.FileExtension}";

            blogImage.Url = urlPath;

            await dbContext.BlogImages.AddAsync(blogImage);
            await dbContext.SaveChangesAsync();

            return mapper.Map<BlogImageDto>(blogImage);
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

        private void ValidateFileUpload(ImageUploadRequestDto request)
        {
            if (request.File == null)
                throw new InvalidOperationException("File is required.");

            if (string.IsNullOrEmpty(request.FileName) || string.IsNullOrEmpty(request.Title))
                throw new InvalidOperationException("File name and title are required.");

            var allowedExtensions = configuration
                .GetSection("AppSettings:AllowedExtensions")
                .Get<string[]>();

            if (!allowedExtensions.Contains(Path.GetExtension(request.File.FileName).ToLower()))
                throw new InvalidOperationException("Unsupported file format.");

            if (request.File.Length > 10485760)
                throw new InvalidOperationException("File size cannot be more than 10MB.");
        }
    }
}
