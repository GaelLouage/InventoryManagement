using Infrastructuur.Entities;
using Infrastructuur.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;

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
        public async Task<IActionResult> PostCategory([FromBody] CategoryEntity category)
        {
            category.Id = (await _repository.GetAllAsync()).Max(x => x.Id) + 1;
            await _repository.AddAsync(category);
            return Ok(await _repository.GetAllAsync());
        }
        [HttpGet("GetCategoryById{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var category = await _repository.GetByIdAsync(x => x.Id == id);
            if(category == null) return NotFound(); 
            return Ok(category);
        }
        [HttpPut]
        public async Task<IActionResult> UpdateCategoryById(int id, [FromBody] CategoryEntity category)
        {
            category.Id = id;
            var cat = await _repository.UpdateAsync(x => x.Id == id , category);
            return Ok(cat);
        }
        [HttpDelete("DeleteCategoryById/{id}")]
        public async Task<IActionResult> DeleteCategoryById(int id)
        {
            await _repository.DeleteAsync(x => x.Id == id);
            return Ok(await _repository.GetAllAsync());
        }
    }
}
