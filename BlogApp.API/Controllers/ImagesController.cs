using BlogApp.API.Models.Domain;
using BlogApp.API.Models.DTO;
using BlogApp.API.Models.Extensions;
using BlogApp.API.Repositories.Interface;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly IImageRepository imageRepository;
        private readonly IConfiguration configuration;

        public ImagesController(IImageRepository imageRepository, IConfiguration configuration)
        {
            this.imageRepository = imageRepository;
            this.configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllImages()
        {
            // Call image repository to get all images
            var images = await imageRepository.GetAll();

            // Convert Domain models to DTOs
            var response = new List<BlogImageDto>();
            foreach (var image in images)
            {
                response.Add(image.ToDto());
            }
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage(
            [FromForm] IFormFile file,
            [FromForm] string fileName,
            [FromForm] string title
        )
        {
            ValidateFileUpload(file);

            if (ModelState.IsValid)
            {
                // File upload
                var blogImage = new BlogImage
                {
                    FileExtension = Path.GetExtension(file.FileName).ToLower(),
                    FileName = fileName,
                    Title = title,
                    DateCreated = DateTime.Now,
                };

                blogImage = await imageRepository.Upload(file, blogImage);

                // Convert Domain Model to DTO using the extension method
                var response = blogImage.ToDto();

                return Ok(response);
            }

            return BadRequest(ModelState);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteImage(Guid id)
        {
            // Delete the image using the image repository
            var deletedImage = await imageRepository.DeleteAsync(id);

            if (deletedImage == null)
            {
                return NotFound();
            }

            return NoContent();
        }

        private void ValidateFileUpload(IFormFile file)
        {
            // later take from appsettings.json
            var allowedExtensions = configuration
                .GetSection("AppSettings:AllowedExtensions")
                .Get<string[]>();

            if (!allowedExtensions.Contains(Path.GetExtension(file.FileName).ToLower()))
            {
                ModelState.AddModelError("file", "Unsupported file format.");
            }
            if (file.Length > 10485760)
            {
                ModelState.AddModelError("file", "File size cannot be more than 10MB.");
            }
        }
    }
}
