using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace NET.Processor.Core.Models.RelationsGraph.Item
{
    public abstract class Item
    {
        [BsonId]
        public ObjectId DatabaseId { get; set; }
        [BsonElement("Id")]
        public int Id { get; set; }
        [BsonElement ("Name")]
        public string Name { get; set; }

        public Guid ProjectId { get; set; }

        public Item()
        {
        }

        public Item(int id, string name, Guid ProjectId)
        {
            Id = id;
            Name = name;
            this.ProjectId = ProjectId;
            DatabaseId = ObjectId.GenerateNewId();
        }

        public Item(Guid ProjectId, string Name)
        {
           this.ProjectId = ProjectId;
           this.Name = Name;
           DatabaseId = ObjectId.GenerateNewId();
        }

        public Item(int id, string name)
        {
            Name = name;
            Id = id;
        }

        public override string ToString()
        {
            return $"{GetType()} {Name}";
        }
    }
}
