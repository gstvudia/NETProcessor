using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace NET.Processor.Core.Models.RelationsGraph.Item
{
    // BSON Annotation means that Database (MongoDB) ignores any properties it cannot recognize
    [BsonIgnoreExtraElements]
    public class File : Item
    {
        [BsonIgnore]
        public Namespace Child { get; set; }
        public Project Parent { get; set; }
        public string FileId { get; set; }
        public File(string FileId, string FileName, string ProjectId, Namespace Child) : base(FileId, FileName, ProjectId) 
        {
            this.FileId = FileId;
            this.Child = Child;
            TypeHierarchy = 1;
        }
    }
}
