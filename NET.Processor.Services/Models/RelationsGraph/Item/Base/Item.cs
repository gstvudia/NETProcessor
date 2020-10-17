using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace NET.Processor.Core.Models.RelationsGraph.Item
{
    public class Item : IDisposable
    {
        public int Id { get; set; }
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
        }

        public Item(string name, TextSpan span)
        {
            Name = name;
            Span = span;
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
