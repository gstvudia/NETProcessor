using Microsoft.CodeAnalysis.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace NET.Processor.Core.Models.RelationsGraph.Item
{
    // BSON Annotation means that Database (MongoDB) ignores any properties it cannot recognize
    [BsonIgnoreExtraElements]
    public class Item : IDisposable
    {
        [BsonId]
        public ObjectId databaseId { get; set; }

        [BsonElement("Id")]
        public int Id { get; set; }
        [BsonElement ("Name")]
        public string Name { get; set; }
        public TextSpan Span { get; set; }
        public Item Parent { get; set; }
        public List<Item> ChildList { get; } = new List<Item>();

        public Item()
        {
        }

        public Item(int id, string name, TextSpan span)
        {
            Id = id;
            Name = name;
            Span = span;

            databaseId = ObjectId.GenerateNewId();
        }

        public Item(string name, TextSpan span)
        {
            Name = name;
            Span = span;

            databaseId = ObjectId.GenerateNewId();
        }

        public override string ToString()
        {
            return $"{GetType()} {Name}";
        }

        public Item Clone()
        {
            return new Item(Name, Span);
        }

        public void Dispose()
        {
            ChildList.Clear();
        }
    }
}
