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
        public List<Method> ChildList { get; set; } = new List<Method>();
        public string Language { get; set; }

        [BsonElement("FileName")]
        public string FileName { get; set; }
        public string FileId { get; set; }

        public Class(string id, string name, string ProjectId, int NamespaceId, string NamespaceName, 
            string FileId, string FileName, string Language, Method child) : base(id, name, ProjectId)
        {
            ChildList.Add(child);
            this.NamespaceId = NamespaceId;
            this.NamespaceName = NamespaceName;
            this.FileName = FileName;
            this.FileId = FileId;
            this.Language = Language;
        }
    }
}
