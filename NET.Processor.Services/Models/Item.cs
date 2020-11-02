using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace NET.Processor.Core.Models
{
    public class Item : IDisposable
    {
        public int Id { get; set; }
        public string Name { get; }

        public ItemType Type { get; }

        public TextSpan Span { get; }

        public Item Parent { get; set; }
        public BlockSyntax Body { get; set; }
        public List<Item> ChildList { get; } = new List<Item>();

        public Item(string name, ItemType type, TextSpan span)
        {
            Name = name;
            Type = type;
            Span = span;
        }

        public Item(int id, string name, ItemType type, TextSpan span)
        {
            Id = id;
            Name = name;
            Type = type;
            Span = span;
        }

        public Item(int id, string name, BlockSyntax body)
        {
            Id = id;
            Name = name;
            Body = body;
        }

        public Item(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public override string ToString()
        {
            return $"{Type} {Name}";
        }

        public Item Clone()
        {
            return new Item(Name, Type, Span);
        }

        public void Dispose()
        {
            ChildList.Clear();
        }
    }
}
