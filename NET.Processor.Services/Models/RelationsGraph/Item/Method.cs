using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Bson.Serialization.Attributes;
using NET.Processor.Core.Models.RelationsGraph.Item;

namespace NET.Processor.Core.Models
{
    public class Method : Item
    {
        public BlockSyntax Body { get; set; }
        public string Language { get; set; }

        public Method(int id, string name,
                      BlockSyntax body, string fileName,
                      string className, string language) : base(id, name, fileName)
        {
            ClassName = className;
            Body = body;
            Language = language;
        }

        [BsonElement("ClassName")]
        public string ClassName { get; set; }

        public Method(int id, string name, BlockSyntax body) : base(id, name) 
        {
            Body = body;
        }

        public Method(int id, string name) : base(id, name) {}
    }
}