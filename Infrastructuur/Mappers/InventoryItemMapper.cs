using Infrastructuur.Dtos;
using Infrastructuur.Entities;
using Infrastructuur.Records;
using Infrastructuur.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructuur.Mappers
{
    public static class InventoryItemMapper
    {
        public static async Task<InventoryItemEntity?> Map(InventoryItemDto entity, InventoryItemMapParams param,
            IRepository<ProductEntity> productRepository,
            IRepository<SupplierEntity> supplierRepository,
            IRepository<CategoryEntity> categoryRepository)
        {
            var validation = await ValidateEntitiesExist(param.CategoryId, param.SupplierId, param.ProductId, productRepository, supplierRepository, categoryRepository);
            if (!validation.valid) return null;
            return new InventoryItemEntity()
            {
                InventoryItemId = param.Id,
                CategoryId = param.CategoryId,
                SupplierId = param.SupplierId,
                ProductId = param.ProductId,
                Quantity = param.Quantity,
                Product = validation.product,
                Supplier = validation.supplier,
                Category = validation.category
            };
        }
        public static async Task<InventoryItemMapParams> MapInventoryRecord(this InventoryItemDto inventoryEntity,IRepository<InventoryItemEntity> inventoryRepository)
        {
            return new InventoryItemMapParams(
                (await inventoryRepository.GetAllAsync()).Max(x => x.InventoryItemId) + 1,
                  inventoryEntity.CategoryId,
                  inventoryEntity.SupplierId,
                  inventoryEntity.ProductId,
                  inventoryEntity.Quantity
          );
        }
        private static async Task<(bool valid, ProductEntity product, SupplierEntity supplier, CategoryEntity category)> ValidateEntitiesExist(int categoryId, int supplierId, int productId,
                                                      IRepository<ProductEntity> productRepository,
                                                      IRepository<SupplierEntity> supplierRepository,
                                                      IRepository<CategoryEntity> categoryRepository)
        {
            // parralel loading
            var product =  productRepository.GetByIdAsync(x => x.ProductId == productId);
            var supplier =  supplierRepository.GetByIdAsync(x => x.SupplierId == supplierId);
            var category =  categoryRepository.GetByIdAsync(x => x.CategoryId == categoryId);
            await Task.WhenAll(product.AsTask(), supplier.AsTask(), category.AsTask());
        
            return (valid:product != null && supplier != null && category != null, product:product.Result, supplier : supplier.Result, category:category.Result);
        }
    }
}
