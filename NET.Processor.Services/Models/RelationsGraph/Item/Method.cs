using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace NET.Processor.Core.Models.RelationsGraph.Item
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

        public ParameterListSyntax ParameterList { get; set; }

        public Method(string id, string name, string ProjectId,
                      BlockSyntax body, ParameterListSyntax ParameterList, string FileId, string FileName,
                      string ClassName, int ClassId, string language) : base(id, name, ProjectId)
        {
            this.ClassName = ClassName;
            this.ClassId = ClassId;
            this.FileName = FileName;
            this.FileId = FileId;
            Body = body;
            this.ParameterList = ParameterList;
            Language = language;
        }

        public Method(string id, string name, BlockSyntax body) : base(id, name) 
        {
            Body = body;
        }

        public Method(string id, string name) : base(id, name) {}
    }
}