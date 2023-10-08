using BlogApp.API.Data;
using BlogApp.API.Models.Domain;
using BlogApp.API.Models.DTO;
using BlogApp.API.Repositories.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BlogApp.API.Models.Extensions;

namespace BlogApp.API.Controllers
{
    //https://localhost:7055/api/categories
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryRepository categoryRepository;

        public CategoriesController(ICategoryRepository categoryRepository)
        {
            this.categoryRepository = categoryRepository;
        }

        [HttpPost]
        [Authorize(Roles = "Writer")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequestDto request)
        {
            var category = request.ToDomainModel();
            await categoryRepository.CreateAsync(category);

            var response = category.ToDto();
            return Ok(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await categoryRepository.GetAllAsync();

            var response = categories.Select(category => category.ToDto()).ToList();
            return Ok(response);
        }

        [HttpGet]
        [Route("{id:Guid}")]
        public async Task<IActionResult> GetCategoryById([FromRoute] Guid id)
        {
            var existingCategory = await categoryRepository.GetByIdAsync(id);

            if (existingCategory is null)
            {
                return NotFound();
            }

            // Use the ToDto extension method to convert the domain model to a DTO
            var response = existingCategory.ToDto();

            return Ok(response);
        }

        [HttpPut]
        [Route("{id:guid}")]
        [Authorize(Roles = "Writer")]
        public async Task<IActionResult> EditCategory(
            [FromRoute] Guid id,
            UpdateCategoryRequestDto request
        )
        {
            var existingCategory = await categoryRepository.GetByIdAsync(id);
            if (existingCategory is null)
            {
                return NotFound();
            }

            // Use the ToDomainModel extension method for UpdateCategoryRequestDto
            var updatedCategory = request.ToDomainModel();
            updatedCategory.Id = id;

            updatedCategory = await categoryRepository.UpdateAsync(updatedCategory);

            var response = updatedCategory.ToDto();
            return Ok(response);
        }

        [HttpDelete]
        [Route("{id:Guid}")]
        [Authorize(Roles = "Writer")]
        public async Task<IActionResult> DeleteCategory([FromRoute] Guid id)
        {
            var category = await categoryRepository.DeleteAsync(id);

            if (category is null)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
