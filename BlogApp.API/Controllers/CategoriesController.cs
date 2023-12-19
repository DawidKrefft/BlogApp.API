using BlogApp.API.Models.DTO;
using BlogApp.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryRepository categoryRepository;

        public CategoriesController(ICategoryRepository categoryRepository)
        {
            this.categoryRepository = categoryRepository;
        }

        /// <summary>
        /// Retrieves a paginated list of all categories.
        /// </summary>
        /// <param name="page">The page number for pagination (default is 1).</param>
        /// <param name="pageSize">The number of categories per page (default is 5).</param>
        /// <returns>Returns a paginated result of categories.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllCategories(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 5
        )
        {
            var paginatedResult = await categoryRepository.GetAllAsync(page, pageSize);
            return Ok(paginatedResult);
        }

        /// <summary>
        /// Retrieves a category by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the category.</param>
        /// <returns>Returns the category if found; otherwise, returns NotFound.</returns>
        [HttpGet("{id:Guid}")]
        public async Task<IActionResult> GetCategoryById([FromRoute] Guid id)
        {
            var existingCategory = await categoryRepository.GetByIdAsync(id);
            return existingCategory != null ? Ok(existingCategory) : NotFound();
        }

        /// <summary>
        /// Creates a new category (requires "Writer" role).
        /// </summary>
        /// <param name="request">The DTO containing information for creating a new category.</param>
        /// <returns>Returns the created category.</returns>
        [HttpPost]
        [Authorize(Roles = "Writer")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequestDto request)
        {
            var category = await categoryRepository.CreateAsync(request);
            return Ok(category);
        }

        /// <summary>
        /// Edits an existing category by its unique identifier (requires "Writer" role).
        /// </summary>
        /// <param name="id">The unique identifier of the category to be edited.</param>
        /// <param name="request">The DTO containing updated information for the category.</param>
        /// <returns>Returns the updated category if successful; otherwise, returns NotFound.</returns>
        [HttpPut("{id:Guid}")]
        [Authorize(Roles = "Writer")]
        public async Task<IActionResult> EditCategory(
            [FromRoute] Guid id,
            UpdateCategoryRequestDto request
        )
        {
            var updatedCategory = await categoryRepository.UpdateAsync(id, request);
            return updatedCategory != null ? Ok(updatedCategory) : NotFound();
        }

        /// <summary>
        /// Deletes a category by its unique identifier (requires "Writer" role).
        /// </summary>
        /// <param name="id">The unique identifier of the category to be deleted.</param>
        /// <returns>Returns NoContent if the category is successfully deleted; otherwise, returns NotFound.</returns>
        [HttpDelete("{id:Guid}")]
        [Authorize(Roles = "Writer")]
        public async Task<IActionResult> DeleteCategory([FromRoute] Guid id)
        {
            var result = await categoryRepository.DeleteAsync(id);
            return result ? NoContent() : NotFound();
        }
    }
}
