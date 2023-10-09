using BlogApp.API.Models.Domain;
using BlogApp.API.Models.DTO;
using BlogApp.API.Repositories.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;

namespace BlogApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryRepository categoryRepository;
        private readonly IMapper mapper;

        public CategoriesController(ICategoryRepository categoryRepository, IMapper mapper)
        {
            this.categoryRepository = categoryRepository;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await categoryRepository.GetAllAsync();

            var response = mapper.Map<List<CategoryDto>>(categories);
            return Ok(response);
        }

        [HttpGet("{id:Guid}")]
        public async Task<IActionResult> GetCategoryById([FromRoute] Guid id)
        {
            var existingCategory = await categoryRepository.GetByIdAsync(id);

            if (existingCategory is null)
            {
                return NotFound();
            }

            var response = mapper.Map<CategoryDto>(existingCategory);
            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "Writer")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequestDto request)
        {
            var category = mapper.Map<Category>(request);
            await categoryRepository.CreateAsync(category);

            var response = mapper.Map<CategoryDto>(category);
            return Ok(response);
        }

        [HttpPut("{id:Guid}")]
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

            mapper.Map(request, existingCategory);

            var updatedCategory = await categoryRepository.UpdateAsync(existingCategory);

            var response = mapper.Map<CategoryDto>(updatedCategory);
            return Ok(response);
        }

        [HttpDelete("{id:Guid}")]
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
