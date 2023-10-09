using AutoMapper;
using BlogApp.API.Models.Domain;
using BlogApp.API.Models.DTO;
using BlogApp.API.Repositories.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogPostsController : ControllerBase
    {
        private readonly IBlogPostRepository blogPostRepository;
        private readonly ICategoryRepository categoryRepository;
        private readonly IMapper mapper;

        public BlogPostsController(
            IBlogPostRepository blogPostRepository,
            ICategoryRepository categoryRepository,
            IMapper mapper
        )
        {
            this.blogPostRepository = blogPostRepository;
            this.categoryRepository = categoryRepository;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBlogPost()
        {
            var blogPosts = await blogPostRepository.GetAllAsync();
            var response = mapper.Map<List<BlogPostDto>>(blogPosts);
            return Ok(response);
        }

        [HttpGet("{id:Guid}")]
        public async Task<IActionResult> GetBlogPostById([FromRoute] Guid id)
        {
            var blogPost = await blogPostRepository.GetByIdAsync(id);

            if (blogPost is null)
            {
                return NotFound();
            }

            var response = mapper.Map<BlogPostDto>(blogPost);
            return Ok(response);
        }

        [HttpGet("{urlHandle}")]
        public async Task<IActionResult> GetBlogPostByUrlHandle([FromRoute] string urlHandle)
        {
            var blogPost = await blogPostRepository.GetByUrlHandleAsync(urlHandle);

            if (blogPost is null)
            {
                return NotFound();
            }

            var response = mapper.Map<BlogPostDto>(blogPost);
            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "Writer")]
        public async Task<IActionResult> CreateBlogPost([FromBody] CreateBlogPostRequestDto request)
        {
            var blogPost = mapper.Map<BlogPost>(request);

            foreach (var categoryGuid in request.Categories)
            {
                var existingCategory = await categoryRepository.GetByIdAsync(categoryGuid);
                if (existingCategory is not null)
                {
                    blogPost.Categories.Add(existingCategory);
                }
            }

            blogPost = await blogPostRepository.CreateAsync(blogPost);

            var response = mapper.Map<BlogPostDto>(blogPost);

            return Ok(response);
        }

        [HttpPut("{id:Guid}")]
        [Authorize(Roles = "Writer")]
        public async Task<IActionResult> UpdateBlogPostById(
            [FromRoute] Guid id,
            UpdateBlogPostRequestDto request
        )
        {
            var blogPost = mapper.Map<BlogPost>(request);
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

            var response = mapper.Map<BlogPostDto>(updatedBlogPost);
            return Ok(response);
        }

        [HttpDelete("{id:Guid}")]
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
