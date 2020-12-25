using Microsoft.CodeAnalysis.Text;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace NET.Processor.Core.Models.RelationsGraph.Item
{
    public class Namespace : Item
    {
        [BsonIgnore]
        public List<Class> ChildList { get; } = new List<Class>();

        [BsonElement("FileName")]
        public string FileName { get; set; }
        public Guid FileId { get; set; }

        public Namespace(int id, string name, Guid FileId, string FileName, List<Class> ChildList) : base(id, name)
        {
            this.FileId = FileId;
            this.FileName = FileName;
            this.ChildList = ChildList;
        }
    }
}
