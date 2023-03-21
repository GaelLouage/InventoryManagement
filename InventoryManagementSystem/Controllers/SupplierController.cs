using Infrastructuur.Dtos;
using Infrastructuur.Entities;
using Infrastructuur.Mappers;
using Infrastructuur.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace InventoryManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SupplierController : ControllerBase
    {
        private readonly IRepository<SupplierEntity> _supplierRepository;
        private readonly IMemoryCache _memoryCache;
        private static CancellationTokenSource _resetCacheToken = new CancellationTokenSource();
        public SupplierController(IRepository<SupplierEntity> supplierRepository, IMemoryCache memoryCache)
        {
            _supplierRepository = supplierRepository;
            _memoryCache = memoryCache;
        }

        [HttpGet("GetAllSuppliers")]
        public async Task<IActionResult> GetAllSuppliers()
        {
            // keeping data in memory
            if (_memoryCache.TryGetValue(_resetCacheToken, out List<SupplierEntity> suppliersCache))
            {
                return Ok(suppliersCache);
            }
            var suppliers = await _supplierRepository.GetAllAsync();
            //set a timespan to keep the data into memory (10min)
            _memoryCache.Set(_resetCacheToken, suppliers, TimeSpan.FromMinutes(10));
            if (suppliers == null) return NotFound();
            return Ok(suppliers);
        }

        [HttpPost("CreateSupplier")]
        public async Task<IActionResult> CreateSupplier([FromBody] SupplierDto supplier)
        {
            var supplierEntity = SupplierMapper.Map(supplier, (await _supplierRepository.GetAllAsync()).Max(x => x.SupplierId) + 1);
            await _supplierRepository.AddAsync(supplierEntity);
            // removes the mem cache to reset it
            _memoryCache.Remove(_resetCacheToken);
            return CreatedAtAction(nameof(GetSupplierById), new { id = supplierEntity.SupplierId }, supplierEntity);
        }

        [HttpGet("GetSupplierById/{id}")]
        public async Task<IActionResult> GetSupplierById(int id)
        {
            if (_memoryCache.TryGetValue(_resetCacheToken, out List<SupplierEntity> supCache))
            {
                var memCategory = supCache.FirstOrDefault(x => x.SupplierId == id);
                if (memCategory == null) return NotFound();
                return Ok(memCategory);
            }
            var supplier = await _supplierRepository.GetByIdAsync(x => x.SupplierId == id);
            _memoryCache.Set(_resetCacheToken, supplier, TimeSpan.FromMinutes(10));
            if (supplier == null) return NotFound();
            return Ok(supplier);
        }

        [HttpPut("UpdateSupplierById/{id}")]
        public async Task<IActionResult> UpdateSupplierById(int id, SupplierDto supplier)
        {
            var existingCategory = await _supplierRepository.GetByIdAsync(x => x.SupplierId == id);
            if (existingCategory == null) return NotFound();
            var UpdateCategory = SupplierMapper.Map(supplier, existingCategory.Id, existingCategory.SupplierId);

            await _supplierRepository.UpdateAsync(x => x.SupplierId == id, UpdateCategory);
            _memoryCache.Remove(_resetCacheToken);
            return NoContent();
        }
        [HttpDelete("DeleteCategoryById/{id}")]
        public async Task<IActionResult> DeleteCategoryById(int id)
        {
            var existingSupplier = await _supplierRepository.GetByIdAsync(x => x.SupplierId == id);
            if (existingSupplier == null) return NotFound();
            _memoryCache.Remove(_resetCacheToken);
            await _supplierRepository.DeleteAsync(x => x.SupplierId == id);
            return NoContent();
        }
    }
}
