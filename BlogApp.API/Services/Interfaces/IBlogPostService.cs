﻿using BlogApp.API.Models.DTO;

namespace BlogApp.API.Services.Interfaces
{
    public interface IBlogPostService
    {
        Task<PaginatedResult<BlogPostDto>> GetAllAsync(int page, int pageSize);
        Task<BlogPostDto> GetByIdAsync(Guid id);
        Task<BlogPostDto> GetByUrlHandleAsync(string urlHandle);
        Task<BlogPostDto> CreateAsync(CreateBlogPostRequestDto request);
        Task<BlogPostDto> UpdateAsync(Guid id, UpdateBlogPostRequestDto request);
        Task<bool> DeleteAsync(Guid id);
    }
}
