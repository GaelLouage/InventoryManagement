using Infrastructuur.Dtos;
using Infrastructuur.Entities;
using Infrastructuur.Mappers;
using Infrastructuur.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Bson;
using Newtonsoft.Json;

namespace InventoryManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IRepository<InventoryItemEntity> _inventoryRepository;
        private readonly IRepository<ProductEntity> _productRepository;
        private readonly IMemoryCache _memoryCache;
        private static CancellationTokenSource _resetCacheToken = new CancellationTokenSource();
        public ProductController(IRepository<InventoryItemEntity> inventoryRepository, IRepository<ProductEntity> productRepository, IMemoryCache memoryCache)
        {
            _inventoryRepository = inventoryRepository;
            _productRepository = productRepository;
            _memoryCache = memoryCache;
            _memoryCache.Remove(_resetCacheToken);
        }
        [HttpGet("GetAllProducts")]
        public async Task<IActionResult> GetAllProducts()
        {
            if(_memoryCache.TryGetValue(_resetCacheToken, out List<ProductEntity> productCache))
            {
                return Ok(productCache);
            }
            var products = await _productRepository.GetAllAsync();
            if (products is null) return NotFound();
            _memoryCache.Set(_resetCacheToken, products, TimeSpan.FromMinutes(10));
            return Ok(products);
        }
        [HttpGet("GetProductById/{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            if (_memoryCache.TryGetValue(_resetCacheToken, out List<ProductEntity> productCache))
            {
                var prodCache = productCache.FirstOrDefault(x => x.ProductId == id);
                return Ok(prodCache);
            }
            var product = await _productRepository.GetByIdAsync(x => x.ProductId == id);
            if (product is null) return NotFound();
            return Ok(product);
        }
        [HttpPut("UpdateProduct/{id}")]
        public async Task<IActionResult> UpdateProduct([FromRoute] int id, [FromBody] ProductDto product)
        {
            var inv = (await _inventoryRepository.GetByIdAsync(x => x.ProductId == id));
            var productToUpdate = await _productRepository.GetByIdAsync(x => x.ProductId == id);
            if(productToUpdate is null) return NotFound();
            productToUpdate = ProductMapper.Map(product, productToUpdate.Id, productToUpdate.ProductId);
            inv.Quantity = productToUpdate.Quantity;
            await _productRepository.UpdateAsync(x => x.ProductId == id , productToUpdate);
            await _inventoryRepository.UpdateAsync(x => x.InventoryItemId == inv.InventoryItemId, inv);
            _memoryCache.Remove(_resetCacheToken);
            return Ok(productToUpdate);
        }
        [HttpPost("CreateProduct")]
        public async Task<IActionResult> CreateProduct([FromBody] ProductDto product)
        {
        
            var productEntity = new ProductEntity();
            productEntity.ProductId = (await _productRepository.GetAllAsync()).Max(x => x.ProductId) + 1;
            productEntity = ProductMapper.Map(product, productEntity.ProductId);
            await _productRepository.AddAsync(productEntity);
            _memoryCache.Remove(_resetCacheToken);
            return CreatedAtAction(nameof(GetProductById), new { id = productEntity.ProductId }, productEntity);
        }
        [HttpDelete("DeleteProduct/{id}")]
        public async Task<IActionResult> DeleteProduct([FromRoute] int id)
        {
            var existingInventoryItem = await _inventoryRepository.GetByIdAsync(x => x.ProductId == id);
            if(existingInventoryItem is null)
            {
                await _productRepository.DeleteAsync(x => x.ProductId == id);
                _memoryCache.Remove(_resetCacheToken);
                return NoContent();
            }
            // delete inventories where id == productID
            await _inventoryRepository.DeleteRangeAsync(x => x.ProductId == id);
            await _productRepository.DeleteAsync(x => x.ProductId == id);
            _memoryCache.Remove(_resetCacheToken);
            return NoContent();
        }
    }
}

