using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using NET.Processor.Core.Services;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

        public List<Item> ChildList { get; } = new List<Item>();

        /// <summary>
        /// This will never be null or blank
        /// </summary>
        public string Content { get; private set; }

        /// <summary>
        /// This will always be a positive integer
        /// </summary>
        public int LineNumber { get; private set; }

        /// <summary>
        /// This may be null since the comment may not exist within a method or property
        /// </summary>
        public MemberDeclarationSyntax MethodOrPropertyIfAny { get; private set; }

        /// <summary>
        /// This may be null since the comment may not exist within an class, interface or struct
        /// </summary>
        public TypeDeclarationSyntax TypeIfAny { get; private set; }

        /// <summary>
        /// This may be null since the comment may not exist within a method or property
        /// </summary>
        public NamespaceDeclarationSyntax NamespaceIfAny { get; private set; }

        public Item(int lineNumber, string content, ItemType type, MemberDeclarationSyntax methodOrPropertyIfAny, TypeDeclarationSyntax typeIfAny, NamespaceDeclarationSyntax namespaceIfAny)
        {
            if (string.IsNullOrEmpty(content))
                throw new ArgumentException("Null/blank content specified");
            if (lineNumber < 1)
                throw new ArgumentOutOfRangeException("lineNumber");

            LineNumber = lineNumber;
            Content = content;
            Type = type;
            MethodOrPropertyIfAny = methodOrPropertyIfAny;
            TypeIfAny = typeIfAny;
            NamespaceIfAny = namespaceIfAny;
        }

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
