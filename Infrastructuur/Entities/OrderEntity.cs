using MongoDB.Bson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructuur.Entities
{
    public class OrderEntity
    {
        [JsonProperty("_id")]
        public ObjectId Id { get; set; }
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderItemEntity>? Items { get; set; }
        public int TotalOrders  =>  Items.Count;
    }
}
