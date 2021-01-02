using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Bson.Serialization.Attributes;
using NET.Processor.Core.Models.RelationsGraph.Item;
using System;
using System.Collections.Generic;

namespace NET.Processor.Core.Models
{
    public class Method : Item
    {
        public BlockSyntax Body { get; set; }
        public string Language { get; set; }
        public string ClassName { get; set; }
        public int ClassId { get; set; }

        [BsonIgnore]
        public List<Method> ChildList { get; } = new List<Method>();

        [BsonElement("FileName")]
        public string FileName { get; set; }
        public string FileId { get; set; }

        public Method(string id, string name, string ProjectId,
                      BlockSyntax body, string FileId, string FileName,
                      string ClassName, int ClassId, string language) : base(id, name, ProjectId)
        {
            this.ClassName = ClassName;
            this.ClassId = ClassId;
            this.FileName = FileName;
            this.FileId = FileId;
            Body = body;
            Language = language;
        }

        public Method(string id, string name, BlockSyntax body) : base(id, name) 
        {
            Body = body;
        }

        public Method(string id, string name) : base(id, name) {}
    }
}