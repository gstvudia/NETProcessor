using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace NET.Processor.Core.Models.RelationsGraph.Item
{
    public abstract class Item
    {
        [BsonId]
        public ObjectId DatabaseId { get; set; }
        [BsonElement("Id")]
        public string Id { get; set; }
        [BsonElement ("Name")]
        public string Name { get; set; }

        [BsonIgnore]
        public List<Comment> CommentList = new List<Comment>();

        [BsonIgnore]
        public string ProjectId { get; set; }

        /// <summary>
        /// This is used to filter by type of node part
        /// </summary>
        public int TypeHierarchy { get; set; }

        /// <summary>
        /// This is used to filter by specific name of a node part
        /// </summary>
        public List<string> GraphStreamGuid = new List<string>();

        public Item()
        {
        }

        public Item(string id)
        {
            Id = id;
            DatabaseId = ObjectId.GenerateNewId();
        }

        public Item(string id, string name, string ProjectId)
        {
            Id = id;
            Name = name;
            this.ProjectId = ProjectId;
            DatabaseId = ObjectId.GenerateNewId();
        }

        public Item(string id, string name)
        {
            Id = id;
            Name = name;
            DatabaseId = ObjectId.GenerateNewId();
        }

        public override string ToString()
        {
            return $"{GetType()} {Name}";
        }
    }
}
