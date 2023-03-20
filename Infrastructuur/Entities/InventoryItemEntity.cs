using MongoDB.Bson;
using Newtonsoft.Json;

namespace Infrastructuur.Entities
{
    public class InventoryItemEntity
    {
        [JsonProperty("_id")]
        public ObjectId Id { get; set; }
        public int InventoryIteId { get; set; }
        public int ProductId { get; set; }
        public int CategoryId { get; set; }
        public int SupplierId { get; set; }
        public int Quantity { get; set; }
        public DateTime DateAdded { get; set; } = DateTime.Now;
        public DateTime LastUpdated { get; set; } = DateTime.Now;
        public ProductEntity Product { get; set; }
        public CategoryEntity Category { get; set; }
        public SupplierEntity Supplier { get; set; }
    }
}
