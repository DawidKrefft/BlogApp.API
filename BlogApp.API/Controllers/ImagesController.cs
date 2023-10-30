using BlogApp.API.Models.DTO;
using BlogApp.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly IImageRepository imageRepository;

        public ImagesController(IImageRepository imageRepository)
        {
            this.imageRepository = imageRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBlogPost(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 5
        )
        {
            var paginatedResult = await imageRepository.GetAllAsync(page, pageSize);
            return Ok(paginatedResult);
        }

        [HttpPost]
        [Authorize(Roles = "Writer")]
        public async Task<IActionResult> UploadImage([FromForm] ImageUploadRequestDto request)
        {
            var blogImage = await imageRepository.Upload(request);
            return Ok(blogImage);
        }

        [HttpDelete("{id:Guid}")]
        [Authorize(Roles = "Writer")]
        public async Task<IActionResult> DeleteImage([FromRoute] Guid id)
        {
            var deletedImage = await imageRepository.DeleteAsync(id);
            return deletedImage != null ? NoContent() : NotFound();
        }
    }
}
