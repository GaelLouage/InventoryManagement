using MongoDB.Bson;
using Newtonsoft.Json;

namespace Infrastructuur.Entities
{
    public class CategoryEntity
    {
        [JsonProperty("_id")]
        public ObjectId Id { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
