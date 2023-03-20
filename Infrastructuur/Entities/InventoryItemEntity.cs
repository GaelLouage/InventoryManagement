namespace Infrastructuur.Entities
{
    public class InventoryItemEntity
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int CategoryId { get; set; }
        public int SupplierId { get; set; }
        public int Quantity { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime LastUpdated { get; set; }
        public ProductEntity Product { get; set; }
        public CategoryEntity Category { get; set; }
        public SupplierEntity Supplier { get; set; }
    }
}
