using Infrastructuur.Entities;
using MongoDB.Bson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructuur.Dtos
{
    public class OrderDto
    {
        [JsonProperty("_id")]
        public ObjectId Id { get; set; }
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderItemEntity> Items { get; set; }
    }
}
