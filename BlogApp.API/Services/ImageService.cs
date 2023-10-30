using AutoMapper;
using BlogApp.API.Data;
using BlogApp.API.Models.Domain;
using BlogApp.API.Models.DTO;
using BlogApp.API.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.API.Services
{
    public class ImageService : IImageRepository
    {
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IMapper mapper;
        private readonly ApplicationDbContext dbContext;
        private readonly IValidator<ImageUploadRequestDto> imageUploadValidator;

        public ImageService(
            IWebHostEnvironment webHostEnvironment,
            IHttpContextAccessor httpContextAccessor,
            IMapper mapper,
            ApplicationDbContext dbContext,
            IValidator<ImageUploadRequestDto> ImageUploadValidator
        )
        {
            this.webHostEnvironment = webHostEnvironment;
            this.httpContextAccessor = httpContextAccessor;
            this.mapper = mapper;
            this.dbContext = dbContext;
            imageUploadValidator = ImageUploadValidator;
        }

        public async Task<PaginatedResult<BlogImageDto>> GetAllAsync(int page, int pageSize)
        {
            try
            {
                pageSize = Math.Min(pageSize, 10);

                var query = dbContext.BlogImages.AsNoTracking();
                var totalItems = await query.CountAsync();

                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                var blogImages = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var blogImageDtos = mapper.Map<List<BlogImageDto>>(blogImages);

                var result = new PaginatedResult<BlogImageDto>
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    Items = blogImageDtos
                };

                return result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve blog images.", ex);
            }
        }

        public async Task<BlogImageDto> Upload([FromForm] ImageUploadRequestDto request)
        {
            var validation = await imageUploadValidator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                var validationErrors = string.Join(
                    ", ",
                    validation.Errors.Select(error => error.ErrorMessage)
                );
                throw new ValidationException(validationErrors);
            }

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

        public async Task<BlogImage> DeleteAsync(Guid id)
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
