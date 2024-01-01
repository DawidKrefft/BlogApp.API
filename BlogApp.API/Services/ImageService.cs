using AutoMapper;
using BlogApp.API.Data;
using BlogApp.API.Exceptions;
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
            if (page == 0 || pageSize == 0)
            {
                throw new InvalidOperationException("Page or PageSize cannot be 0");
            }

            try
            {
                pageSize = Math.Min(pageSize, 10);
                var query = dbContext.BlogImages.AsNoTracking();
                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                if (totalItems != 0 && page > totalPages)
                {
                    throw new BadRequestException($"Page cannot be greater than {totalPages}");
                }

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
            catch (BadRequestException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Unexpected error occurred", e);
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
            _ = existingImage ?? throw new NotFoundException("Image not found.");

            // Retrieve the file path from the entity
            var imagePath = Path.Combine(
                webHostEnvironment.ContentRootPath,
                "Images",
                $"{existingImage.FileName}{existingImage.FileExtension}"
            );

            try
            {
                dbContext.BlogImages.Remove(existingImage);
                await dbContext.SaveChangesAsync();
                File.Delete(imagePath);
            }
            catch (Exception e)
            {
                await Console.Out.WriteLineAsync($"{e}");
            }

            return existingImage;
        }
    }
}
