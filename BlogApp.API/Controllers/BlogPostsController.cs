using BlogApp.API.Models.DTO;
using BlogApp.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogPostsController : ControllerBase
    {
        private readonly IBlogPostRepository blogPostRepository;

        public BlogPostsController(IBlogPostRepository blogPostRepository)
        {
            this.blogPostRepository = blogPostRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBlogPost(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 5
        )
        {
            var paginatedResult = await blogPostRepository.GetAllAsync(page, pageSize);
            return Ok(paginatedResult);
        }

        [HttpGet("{id:Guid}")]
        public async Task<IActionResult> GetBlogPostById([FromRoute] Guid id)
        {
            var blogPost = await blogPostRepository.GetByIdAsync(id);
            return blogPost != null ? Ok(blogPost) : NotFound();
        }

        [HttpGet("{urlHandle}")]
        public async Task<IActionResult> GetBlogPostByUrlHandle([FromRoute] string urlHandle)
        {
            var blogPost = await blogPostRepository.GetByUrlHandleAsync(urlHandle);
            return blogPost != null ? Ok(blogPost) : NotFound();
        }

        [HttpPost]
        [Authorize(Roles = "Writer")]
        public async Task<IActionResult> CreateBlogPost([FromBody] CreateBlogPostRequestDto request)
        {
            var blogPost = await blogPostRepository.CreateAsync(request);
            return Ok(blogPost);
        }

        [HttpPut("{id:Guid}")]
        [Authorize(Roles = "Writer")]
        public async Task<IActionResult> UpdateBlogPostById(
            [FromRoute] Guid id,
            [FromBody] UpdateBlogPostRequestDto request
        )
        {
            var updatedBlogPost = await blogPostRepository.UpdateAsync(id, request);
            return updatedBlogPost != null ? Ok(updatedBlogPost) : NotFound();
        }

        [HttpDelete("{id:Guid}")]
        [Authorize(Roles = "Writer")]
        public async Task<IActionResult> DeleteBlogPost([FromRoute] Guid id)
        {
            var result = await blogPostRepository.DeleteAsync(id);
            return result ? NoContent() : NotFound();
        }
    }
}
