using MongoDB.Bson;
using Newtonsoft.Json;

namespace Infrastructuur.Entities
{
    public class CustomerEntity
    {
        [JsonProperty("_id")]
        public ObjectId Id { get; set; }
        public int CustomerId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
    }
}
