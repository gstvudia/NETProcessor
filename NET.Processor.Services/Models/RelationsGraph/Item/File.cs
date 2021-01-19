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
        public List<Namespace> ChildList { get; set; }
        public string FileId { get; set; }
        public File(string FileId, string FileName, string ProjectId, List<Namespace> ChildList) : base(FileId, FileName, ProjectId) 
        {
            this.FileId = FileId;
            this.ChildList = ChildList;
            TypeHierarchy = 1;
        }
    }
}
