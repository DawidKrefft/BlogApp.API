using BlogApp.API.Models.DTO;
using BlogApp.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly IImageService imageService;

        public ImagesController(IImageService imageService)
        {
            this.imageService = imageService;
        }

        /// <summary>
        /// Retrieves a paginated list of all blog images.
        /// </summary>
        /// <param name="page">The page number for pagination (default is 1).</param>
        /// <param name="pageSize">The number of blog images per page (default is 5).</param>
        /// <returns>Returns a paginated result of blog images.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllBlogPost(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 5
        )
        {
            var paginatedResult = await imageService.GetAllAsync(page, pageSize);
            return Ok(paginatedResult);
        }

        /// <summary>
        /// Uploads a new blog image (requires "Writer" role).
        /// </summary>
        /// <param name="request">The DTO containing information for uploading a new blog image.</param>
        /// <returns>Returns the uploaded blog image.</returns>
        [HttpPost]
        [Authorize(Roles = "Writer")]
        public async Task<IActionResult> UploadImage([FromForm] ImageUploadRequestDto request)
        {
            var blogImage = await imageService.Upload(request);
            return Ok(blogImage);
        }

        /// <summary>
        /// Deletes a blog image by its unique identifier (requires "Writer" role).
        /// </summary>
        /// <param name="id">The unique identifier of the blog image to be deleted.</param>
        /// <returns>Returns NoContent if the blog image is successfully deleted; otherwise, returns NotFound.</returns>
        [HttpDelete("{id:Guid}")]
        [Authorize(Roles = "Writer")]
        public async Task<IActionResult> DeleteImage([FromRoute] Guid id)
        {
            var deletedImage = await imageService.DeleteAsync(id);
            return deletedImage != null ? NoContent() : NotFound();
        }
    }
}
