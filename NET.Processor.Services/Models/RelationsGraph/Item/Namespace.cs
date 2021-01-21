using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace NET.Processor.Core.Models.RelationsGraph.Item
{
    public class Namespace : Item
    {
        [BsonIgnore]
        public List<Class> ChildList { get; } = new List<Class>();
        public File Parent { get; set; }
        [BsonElement("FileName")]
        public string FileName { get; set; }
        public string FileId { get; set; }

        public Namespace(string id, string name, string ProjectId, string FileId, string FileName) : base(id, name, ProjectId)
        {
            this.FileId = FileId;
            this.FileName = FileName;
            TypeHierarchy = 2;
        }

        public void AddRangeChild(List<Class> ChildList)
        {
            this.ChildList.AddRange(ChildList);
        }
    }
}
