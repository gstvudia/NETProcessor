using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace NET.Processor.Core.Models.RelationsGraph.Item
{
    // BSON Annotation means that Database (MongoDB) ignores any properties it cannot recognize
    [BsonIgnoreExtraElements]
    public class Project : Item
    {
        [BsonIgnore]
        public List<File> ChildList { get; } = new List<File>();
        public Project(Guid guid, string name, List<File> ChildList) : base(guid, name) 
        {
            this.ChildList = ChildList;
        }
    }
}
