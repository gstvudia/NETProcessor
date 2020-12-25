using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace NET.Processor.Core.Models.RelationsGraph.Item
{
    // BSON Annotation means that Database (MongoDB) ignores any properties it cannot recognize
    [BsonIgnoreExtraElements]
    public class File : Item
    {
        public Guid FileId { get; set; }
        public File(Guid FileId, string fileName) : base(fileName) 
        {
            this.FileId = FileId;
        }
    }
}
