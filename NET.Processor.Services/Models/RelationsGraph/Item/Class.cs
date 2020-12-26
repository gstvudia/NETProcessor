using Microsoft.CodeAnalysis.Text;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace NET.Processor.Core.Models.RelationsGraph.Item
{
    public class Class : Item
    {
        public int NamespaceId { get; set; }
        public string NamespaceName { get; set; }

        [BsonIgnore]
        public List<Method> ChildList { get; } = new List<Method>();
        public string Language { get; set; }

        [BsonElement("FileName")]
        public string FileName { get; set; }
        public Guid FileId { get; set; }

        public Class(int id, string name, Guid ProjectId, int NamespaceId, string NamespaceName, 
            Guid FileId, string FileName, string Language, List<Method> ChildList) : base(id, name, ProjectId)
        {
            this.ChildList = ChildList;
            this.NamespaceId = NamespaceId;
            this.NamespaceName = NamespaceName;
            this.FileName = FileName;
            this.FileId = FileId;
            this.Language = Language;
        }
    }
}
