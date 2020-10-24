using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace NET.Processor.Core.Models.RelationsGraph.Item
{
    // BSON Annotation means that Database (MongoDB) ignores any properties it cannot recognize
    [BsonIgnoreExtraElements]
    public class NodeProject : Item
    {
        [BsonElement("Guid")]
        public Guid Guid { get; set; }

        public NodeProject(Guid guid, string name)
        {
            Guid = guid;
            Name = name;
        }
    }
}
