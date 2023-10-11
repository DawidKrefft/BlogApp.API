using AutoMapper;
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
        private readonly IMapper mapper;
        private readonly IConfiguration configuration;

        public ImagesController(
            IImageRepository imageRepository,
            IMapper mapper,
            IConfiguration configuration
        )
        {
            this.imageRepository = imageRepository;
            this.mapper = mapper;
            this.configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllImages()
        {
            var images = await imageRepository.GetAll();
            var response = mapper.Map<List<BlogImageDto>>(images);
            return Ok(response);
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
