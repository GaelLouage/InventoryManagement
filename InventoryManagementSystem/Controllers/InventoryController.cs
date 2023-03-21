using Infrastructuur.Dtos;
using Infrastructuur.Entities;
using Infrastructuur.Mappers;
using Infrastructuur.Records;
using Infrastructuur.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace InventoryManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly IRepository<InventoryItemEntity> _inventoryRepository;
        private readonly IRepository<ProductEntity> _productRepository;
        private readonly IRepository<CategoryEntity> _categoryRepository;
        private readonly IRepository<SupplierEntity> _supplierRepository;
        private readonly IMemoryCache _inventoryCache;
        private static CancellationTokenSource _resetCacheToken = new CancellationTokenSource();
        public InventoryController(IRepository<InventoryItemEntity> inventoryRepository, IMemoryCache inventoryCache, IRepository<ProductEntity> productRepository, IRepository<CategoryEntity> categoryRepository, IRepository<SupplierEntity> supplierRepository)
        {
            _inventoryRepository = inventoryRepository;
            _inventoryCache = inventoryCache;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _supplierRepository = supplierRepository;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllInventoryItems()
        {
            if (_inventoryCache.TryGetValue(_resetCacheToken, out List<InventoryItemEntity> inventory))
            {
                return Ok(inventory);
            }
            var invetoryItems = await _inventoryRepository.GetAllAsync();
            if (invetoryItems is null) return NotFound();
            _inventoryCache.Set(_resetCacheToken, invetoryItems, TimeSpan.FromMinutes(10));
            return Ok(invetoryItems);
        }
        [HttpGet("GetInventoryById/{id}")]
        public async Task<IActionResult> GetInventoryById(int id)
        {
            if (_inventoryCache.TryGetValue(_resetCacheToken, out List<InventoryItemEntity> inventoryCache))
            {
                var inventoryItemFromCache = inventoryCache.FirstOrDefault(x => x.InventoryItemId == id);
                if (inventoryItemFromCache is null) return NotFound();
                return Ok(inventoryCache);
            }
            var inventory = await _inventoryRepository.GetByIdAsync(x => x.InventoryItemId == id);
            if (inventory is null) return NotFound();
            return Ok(inventory);
        }
        [HttpPost]
        public async Task<IActionResult> CreateInventoryItem([FromBody] InventoryItemDto inventoryEntity)
        {
            var inventoryItemToAdd = await InventoryItemMapper.Map(inventoryEntity, await inventoryEntity.MapInventoryRecord(_inventoryRepository), _productRepository, _supplierRepository, _categoryRepository);
            // removes the mem cache to reste it with new values
             await _inventoryRepository.AddAsync(inventoryItemToAdd);
            _inventoryCache.Remove(_resetCacheToken);
            return CreatedAtAction(nameof(GetInventoryById), new { id = inventoryItemToAdd.CategoryId }, inventoryItemToAdd);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInventoryItem([FromRoute]int id, [FromBody] InventoryItemDto inventoryEntity)
        {
            var inv = await _inventoryRepository.GetByIdAsync(x => x.InventoryItemId == id);
            var inventoryItemToAdd = await InventoryItemMapper.Map(inventoryEntity, await inventoryEntity.MapInventoryRecord(_inventoryRepository), _productRepository, _supplierRepository, _categoryRepository);
            inventoryItemToAdd.InventoryItemId = id;
            inventoryItemToAdd.Id = inv.Id;
            
            await _inventoryRepository.UpdateAsync(x => x.InventoryItemId == id, inventoryItemToAdd);
            _inventoryCache.Remove(_resetCacheToken);

            return Ok(inventoryItemToAdd);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInventoryItemDto(int id)
        {
            await _inventoryRepository.DeleteAsync(x => x.InventoryItemId == id);
            _inventoryCache.Remove(_resetCacheToken);
            return NoContent();
        }
    }
}
