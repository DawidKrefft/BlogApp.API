using BlogApp.API.Models.Domain;
using BlogApp.API.Models.DTO;
using BlogApp.API.Models.Extensions;
using BlogApp.API.Repositories.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogPostsController : ControllerBase
    {
        private readonly IBlogPostRepository blogPostRepository;
        private readonly ICategoryRepository categoryRepository;

        public BlogPostsController(
            IBlogPostRepository blogPostRepository,
            ICategoryRepository categoryRepository
        )
        {
            this.blogPostRepository = blogPostRepository;
            this.categoryRepository = categoryRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBlogPost()
        {
            var blogPosts = await blogPostRepository.GetAllAsync();
            var response = blogPosts.Select(blogPost => blogPost.ToDto()).ToList();
            return Ok(response);
        }

        [HttpGet]
        [Route("{id:Guid}")]
        public async Task<IActionResult> GetBlogPostById([FromRoute] Guid id)
        {
            var blogPost = await blogPostRepository.GetByIdAsync(id);

            if (blogPost is null)
            {
                return NotFound();
            }

            var response = blogPost.ToDto();
            return Ok(response);
        }

        [HttpGet]
        [Route("{urlHandle}")]
        public async Task<IActionResult> GetBlogPostByUrlHandle([FromRoute] string urlHandle)
        {
            var blogPost = await blogPostRepository.GetByUrlHandleAsync(urlHandle);

            if (blogPost is null)
            {
                return NotFound();
            }

            var response = blogPost.ToDto();
            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "Writer")]
        public async Task<IActionResult> CreateBlogPost(CreateBlogPostRequestDto request)
        {
            var blogPost = request.ToDomainModel();

            foreach (var categoryGuid in request.Categories)
            {
                var existingCategory = await categoryRepository.GetByIdAsync(categoryGuid);
                if (existingCategory != null)
                {
                    blogPost.Categories.Add(existingCategory);
                }
            }

            blogPost = await blogPostRepository.CreateAsync(blogPost);

            var response = blogPost.ToDto();

            return Ok(response);
        }

        [HttpPut]
        [Route("{id:Guid}")]
        [Authorize(Roles = "Writer")]
        public async Task<IActionResult> UpdateBlogPostById(
            [FromRoute] Guid id,
            UpdateBlogPostRequestDto request
        )
        {
            var blogPost = request.ToDomainModel();
            blogPost.Id = id;

            foreach (var categoryGuid in request.Categories)
            {
                var existingCategory = await categoryRepository.GetByIdAsync(categoryGuid);

                if (existingCategory != null)
                {
                    blogPost.Categories.Add(existingCategory);
                }
            }

            var updatedBlogPost = await blogPostRepository.UpdateAsync(blogPost);

            if (updatedBlogPost == null)
            {
                return NotFound();
            }

            var response = updatedBlogPost.ToDto();
            return Ok(response);
        }

        [HttpDelete]
        [Route("{id:Guid}")]
        [Authorize(Roles = "Writer")]
        public async Task<IActionResult> DeleteBlogPost([FromRoute] Guid id)
        {
            var deletedBlogPost = await blogPostRepository.DeleteAsync(id);

            if (deletedBlogPost == null)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
