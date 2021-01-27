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
        public Class Parent { get; set; }

        [BsonElement("FileName")]
        public string FileName { get; set; }
        public string FileId { get; set; }
        public List<string> ParameterList { get; set; }
        public string ReturnType { get; set; }

        public Method(string id, string name, string ProjectId,
                      BlockSyntax body, List<string> ParameterList, string ReturnType, string FileId, string FileName,
                      string ClassName, int ClassId, string language) : base(id, name, ProjectId)
        {
            this.ClassName = ClassName;
            this.ClassId = ClassId;
            this.FileName = FileName;
            this.FileId = FileId;
            Body = body;
            this.ParameterList = ParameterList;
            this.ReturnType = ReturnType;
            Language = language;
            TypeHierarchy = 5; 
        }

        public Method(string id, string name, BlockSyntax body) : base(id, name) 
        {
            Body = body;
            TypeHierarchy = 5;
        }

        public Method(string id, string name) : base(id, name) {
            TypeHierarchy = 5;
        }
    }
}