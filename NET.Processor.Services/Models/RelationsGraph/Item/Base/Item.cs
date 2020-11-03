using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace NET.Processor.Core.Models.RelationsGraph.Item
{
    public class Item : IDisposable
    {
        [BsonId]
        public ObjectId databaseId { get; set; }
        [BsonElement("Id")]
        public int Id { get; set; }
        [BsonElement ("Name")]
        public string Name { get; set; }

        [BsonIgnore]
        public Item Parent { get; set; }

        [BsonIgnore]
        public List<Item> ChildList { get; } = new List<Item>();

        public Item()
        {
        }

        public Item(int id, string name)
        {
            Id = id;
            Name = name;

            databaseId = ObjectId.GenerateNewId();
        }

        public Item(string name)
        {
            Name = name;

            databaseId = ObjectId.GenerateNewId();
        }

        public override string ToString()
        {
            return $"{GetType()} {Name}";
        }

        public Item Clone()
        {
            return new Item(Name);
        }

        public void Dispose()
        {
            ChildList.Clear();
        }
    }
}
