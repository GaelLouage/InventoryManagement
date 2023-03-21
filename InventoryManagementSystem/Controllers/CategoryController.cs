using Infrastructuur.Entities;
using Infrastructuur.Helpers;
using Infrastructuur.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualBasic;
using MongoDB.Bson;

namespace InventoryManagementSystem.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CategoryController : ControllerBase 
    {
        private readonly IRepository<CategoryEntity> _repository;
        // caching categories
        private readonly IMemoryCache _memoryCache;
        private static CancellationTokenSource _resetCacheToken = new CancellationTokenSource();
        public CategoryController(IRepository<CategoryEntity> repository, IMemoryCache memoryCache)
        {
            _repository = repository;
            _memoryCache = memoryCache;
        }
        [HttpGet("GetAllCategories")]
        public async Task<IActionResult> GetAllCategories()
        {
            // keeping data in memory
            if (_memoryCache.TryGetValue(_resetCacheToken, out List<CategoryEntity> cat))
            {
                return Ok(cat);
            }
            var categories = await _repository.GetAllAsync();
            //set a timespan to keep the data into memory (10min)
            _memoryCache.Set(_resetCacheToken, categories, TimeSpan.FromMinutes(10));
            if (categories == null) return NotFound();
            return Ok(categories);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryEntity category)
        {
            category.CategoryId = (await _repository.GetAllAsync()).Max(x => x.CategoryId) + 1;
            await _repository.AddAsync(category);
            // removes the mem cache to reset it
            _memoryCache.Remove(_resetCacheToken);
            return CreatedAtAction(nameof(GetCategoryById), new { id = category.CategoryId }, category);
        }

        [HttpGet("GetCategoryById{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            if (_memoryCache.TryGetValue(_resetCacheToken, out List<CategoryEntity> cat))
            {
                var memCategory = cat.FirstOrDefault(x => x.CategoryId == id);
                if(memCategory == null) return NotFound();  
                return Ok(memCategory);
            }
            var category = await _repository.GetByIdAsync(x => x.CategoryId == id);
            _memoryCache.Set(_resetCacheToken, category, TimeSpan.FromMinutes(10));
            if (category == null) return NotFound(); 
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
            _memoryCache.Remove(_resetCacheToken);
            return NoContent();
        }
        [HttpDelete("DeleteCategoryById/{id}")]
        public async Task<IActionResult> DeleteCategoryById(int id)
        {
            var existingCategory = await _repository.GetByIdAsync(x => x.CategoryId == id);
            if (existingCategory == null) return NotFound();
            _memoryCache.Remove(_resetCacheToken);
            await _repository.DeleteAsync(x => x.CategoryId == id);
            return NoContent();
        }
    }
}
