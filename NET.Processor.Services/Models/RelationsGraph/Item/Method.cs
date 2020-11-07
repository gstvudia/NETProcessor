using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Bson.Serialization.Attributes;
using NET.Processor.Core.Models.RelationsGraph.Item;

namespace NET.Processor.Core.Models
{
    public class Method : Item
    {
        [BsonIgnore]
        public BlockSyntax Body { get; set; }

        public Method(int id, string name, BlockSyntax body) : base(id, name)
        {
            this.Body = body;
        }
        public Method(int id, string name) : base(id, name){}

        

    }
}