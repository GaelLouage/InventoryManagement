using DocumentFormat.OpenXml.Drawing.Charts;
using Infrastructuur.Entities;
using InventoryManagementForms.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagementForms.Helpers
{
    public static class OrderDictionary
    {
        public static readonly Dictionary<Inventory, Func<InventoryItemEntity, object>> SortingOptionsInventory = new()
                              {
                              { Inventory.InventoryItemId, x => x.InventoryItemId },
                              { Inventory.CategoryId, x => x.CategoryId },
                              { Inventory.ProductId, x => x.ProductId },
                              { Inventory.SupplierId, x => x.SupplierId },
                              { Inventory.Quantity, x => x.Quantity },
                              { Inventory.DateAdded, x => x.DateAdded },
                              { Inventory.LastUpdated, x => x.LastUpdated },
                              { Inventory.Product, x => x.Product.Name },
                              { Inventory.Category, x => x.Category.Name },
                              { Inventory.Supplier, x => x.Supplier.Name }
         };
        public static readonly Dictionary<ProductOrder, Func<ProductEntity, object>> SortingOptionsProduct = new()
        {
                                  { ProductOrder.Name, x => x.Name },
                                  { ProductOrder.Quantity, x => x.Quantity },
                                  { ProductOrder.Price, x => x.Price },
                                  { ProductOrder.Description, x => x.Description },
                                  { ProductOrder.ProductId, x => x.ProductId }
         };

        public static readonly Dictionary<Supplier, Func<SupplierEntity, object>> SortingOptionsSupplier = new()
        {
                                  { Supplier.Name, x => x.Name },
                                  { Supplier.Address, x => x.Address },
                                  { Supplier.Phone, x => x.Phone },
                                  { Supplier.Email, x => x.Email },
                                  {Supplier.SupplierId, x => x.SupplierId}
        };

        public static readonly Dictionary<Category, Func<CategoryEntity, object>> SortingOptionsCategory = new()
        {
                                  { Category.Name, x => x.Name },
                                  { Category.Description, x => x.Description },
                                  {Category.CategoryId, x => x.CategoryId}
         };
        public static readonly Dictionary<UserS, Func<UserEntity, object>> SortingOptionsUser = new()
        {
                                  { UserS.Name, x => x.Name },
                                  { UserS.UserName, x => x.UserName },
                                  {UserS.Email, x => x.Email}
         };
    }
}
