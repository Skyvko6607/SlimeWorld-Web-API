using MongoDB.Bson;

namespace SkyBlockWebAPI.Models
{
    public class BaseEntity
    {
        public ObjectId Id { get; set; }
    }
}
