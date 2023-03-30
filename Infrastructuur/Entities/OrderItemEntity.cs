using MongoDB.Bson;
using Newtonsoft.Json;

namespace Infrastructuur.Entities
{
    public class OrderItemEntity
    {
        public int OrderItemId { get; set; }
        public int InventoryItemId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
