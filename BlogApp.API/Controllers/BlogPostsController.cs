﻿using BlogApp.API.Models.DTO;
using BlogApp.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogPostsController : ControllerBase
    {
        private readonly IBlogPostService blogPostService;

        public BlogPostsController(IBlogPostService blogPostService)
        {
            this.blogPostService = blogPostService;
        }

        /// <summary>
        /// Retrieves a paginated list of all blog posts.
        /// </summary>
        /// <param name="page">The page number for pagination (default is 1).</param>
        /// <param name="pageSize">The number of blog posts per page (default is 5).</param>
        /// <returns>Returns a paginated result of blog posts.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllBlogPost(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 5
        )
        {
            var paginatedResult = await blogPostService.GetAllAsync(page, pageSize);
            return Ok(paginatedResult);
        }

        /// <summary>
        /// Retrieves a blog post by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the blog post.</param>
        /// <returns>Returns the blog post if found; otherwise, returns NotFound.</returns>
        [HttpGet("{id:Guid}")]
        public async Task<IActionResult> GetBlogPostById([FromRoute] Guid id)
        {
            var blogPost = await blogPostService.GetByIdAsync(id);
            return blogPost != null ? Ok(blogPost) : NotFound();
        }

        /// <summary>
        /// Retrieves a blog post by its URL handle.
        /// </summary>
        /// <param name="urlHandle">The URL handle of the blog post.</param>
        /// <returns>Returns the blog post if found; otherwise, returns NotFound.</returns>
        [HttpGet("{urlHandle}")]
        public async Task<IActionResult> GetBlogPostByUrlHandle([FromRoute] string urlHandle)
        {
            var blogPost = await blogPostService.GetByUrlHandleAsync(urlHandle);
            return blogPost != null ? Ok(blogPost) : NotFound();
        }

        /// <summary>
        /// Creates a new blog post (requires "Writer" role).
        /// </summary>
        /// <param name="request">The DTO containing information for creating a new blog post.</param>
        /// <returns>Returns the created blog post.</returns>
        [HttpPost]
        [Authorize(Roles = "Writer")]
        public async Task<IActionResult> CreateBlogPost([FromBody] CreateBlogPostRequestDto request)
        {
            var blogPost = await blogPostService.CreateAsync(request);
            return CreatedAtAction(nameof(GetBlogPostById), new { id = blogPost.Id }, blogPost);
        }

        /// <summary>
        /// Updates an existing blog post by its unique identifier (requires "Writer" role).
        /// </summary>
        /// <param name="id">The unique identifier of the blog post to be updated.</param>
        /// <param name="request">The DTO containing updated information for the blog post.</param>
        /// <returns>Returns the updated blog post if successful; otherwise, returns NotFound.</returns>
        [HttpPut("{id:Guid}")]
        [Authorize(Roles = "Writer")]
        public async Task<IActionResult> UpdateBlogPostById(
            [FromRoute] Guid id,
            [FromBody] UpdateBlogPostRequestDto request
        )
        {
            var updatedBlogPost = await blogPostService.UpdateAsync(id, request);
            return updatedBlogPost != null ? Ok(updatedBlogPost) : NotFound();
        }

        /// <summary>
        /// Deletes a blog post by its unique identifier (requires "Writer" role).
        /// </summary>
        /// <param name="id">The unique identifier of the blog post to be deleted.</param>
        /// <returns>Returns NoContent if the blog post is successfully deleted; otherwise, returns NotFound.</returns>
        [HttpDelete("{id:Guid}")]
        [Authorize(Roles = "Writer")]
        public async Task<IActionResult> DeleteBlogPost([FromRoute] Guid id)
        {
            var result = await blogPostService.DeleteAsync(id);
            return result ? NoContent() : NotFound();
        }
    }
}
