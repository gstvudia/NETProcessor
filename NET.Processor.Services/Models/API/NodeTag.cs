using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NET.Processor.Core.Models.API
{
    /// <summary>
    /// User Tags assigned to nodes from Frontend
    /// </summary>
    public class NodeTag
    {
        [BsonId]
        public ObjectId DatabaseId { get; set; }
        [BsonElement("Id")]
        public string Id { get; set; }
        public string name { get; set; }
    }
}