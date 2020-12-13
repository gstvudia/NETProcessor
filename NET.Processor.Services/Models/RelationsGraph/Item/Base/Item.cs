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
        public ObjectId DatabaseId { get; set; }
        [BsonElement("Id")]
        public int Id { get; set; }
        [BsonElement ("Name")]
        public string Name { get; set; }

        [BsonIgnore]
        public Item Parent { get; set; }

        [BsonIgnore]
        public List<Method> ChildList { get; } = new List<Method>();

        [BsonElement("FileName")]
        public string FileName { get; set; }

        public Item()
        {
        }

        public Item(int id, string name)
        {
            Id = id;
            Name = name;
            DatabaseId = ObjectId.GenerateNewId();
        }

        public Item(int id, string name, string fileName)
        {
            Id = id;
            Name = name;
            FileName = fileName;
            DatabaseId = ObjectId.GenerateNewId();
        }

        public Item(string name)
        {
            Name = name;

            DatabaseId = ObjectId.GenerateNewId();
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
