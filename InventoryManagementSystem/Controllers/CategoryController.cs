using Infrastructuur.Entities;
using Infrastructuur.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using MongoDB.Bson;

namespace InventoryManagementSystem.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CategoryController : ControllerBase 
    {
        private readonly IRepository<CategoryEntity> _repository;

        public CategoryController(IRepository<CategoryEntity> repository)
        {
            _repository = repository;
        }
        [HttpGet("GetAllCategories")]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _repository.GetAllAsync();
            if(categories == null) return NotFound();
            return Ok(categories);
        }
        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryEntity category)
        {
            category.CategoryId = (await _repository.GetAllAsync()).Max(x => x.CategoryId) + 1;
            await _repository.AddAsync(category);
            return CreatedAtAction(nameof(GetCategoryById), new { id = category.CategoryId }, category);
        }
        [HttpGet("GetCategoryById{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var category = await _repository.GetByIdAsync(x => x.CategoryId == id);
            if(category == null) return NotFound(); 
            return Ok(category);
        }
        [HttpPut("id")]
        public async Task<IActionResult> UpdateCategoryById(int id,  CategoryEntity category)
        {
            var existingCategory = await _repository.GetByIdAsync(x => x.CategoryId == id);
            if (existingCategory == null) return NotFound();

            category.CategoryId = id;
            category.Id = existingCategory.Id;
            await _repository.UpdateAsync(x => x.CategoryId == id, category);

            return NoContent();
        }
        [HttpDelete("DeleteCategoryById/{id}")]
        public async Task<IActionResult> DeleteCategoryById(int id)
        {
            var existingCategory = await _repository.GetByIdAsync(x => x.CategoryId == id);
            if (existingCategory == null) return NotFound();

            await _repository.DeleteAsync(x => x.CategoryId == id);
            return NoContent();
        }
    }
}
