using MongoDB.Bson;
using Newtonsoft.Json;

namespace Infrastructuur.Entities
{
    public class SupplierEntity
    {
        [JsonProperty("_id")]
        public ObjectId Id { get; set; }
        public int SupplierId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
    }
}
