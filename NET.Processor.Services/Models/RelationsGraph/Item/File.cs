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
        public Guid FileId { get; set; }
        public File(Guid FileId, string FileName, Guid ProjectId, List<Namespace> ChildList) : base(ProjectId, FileName) 
        {
            this.FileId = FileId;
            this.ChildList = ChildList;
        }
    }
}
