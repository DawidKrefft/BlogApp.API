using AutoMapper;
using BlogApp.API.Exceptions;
using BlogApp.API.Models.Domain;
using BlogApp.API.Models.DTO;
using BlogApp.API.Repositories.Interfaces;
using BlogApp.API.Services.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.API.Services
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IImageRepository imageRepository;
        private readonly IMapper mapper;
        private readonly IValidator<ImageUploadRequestDto> imageUploadValidator;

        public ImageService(
            IWebHostEnvironment webHostEnvironment,
            IHttpContextAccessor httpContextAccessor,
            IImageRepository imageRepository,
            IMapper mapper,
            IValidator<ImageUploadRequestDto> ImageUploadValidator
        )
        {
            this.webHostEnvironment = webHostEnvironment;
            this.httpContextAccessor = httpContextAccessor;
            this.imageRepository = imageRepository;
            this.mapper = mapper;
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
                var query = await imageRepository.GetAllAsync();
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

            await imageRepository.AddAsync(blogImage);

            return mapper.Map<BlogImageDto>(blogImage);
        }

        public async Task<BlogImage> DeleteAsync(Guid id)
        {
            var existingImage = await imageRepository.GetByIdAsync(id);
            _ = existingImage ?? throw new NotFoundException("Image not found.");

            // Retrieve the file path from the entity
            var imagePath = Path.Combine(
                webHostEnvironment.ContentRootPath,
                "Images",
                $"{existingImage.FileName}{existingImage.FileExtension}"
            );

            try
            {
                await imageRepository.DeleteAsync(existingImage);
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
