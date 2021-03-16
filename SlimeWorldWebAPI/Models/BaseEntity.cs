using MongoDB.Bson;

namespace SlimeWorldWebAPI.Models
{
    public class BaseEntity
    {
        public ObjectId Id { get; set; }
    }
}
