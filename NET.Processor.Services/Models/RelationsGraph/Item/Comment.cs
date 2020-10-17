using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace NET.Processor.Core.Models.RelationsGraph.Item
{
    public class Comment : Item
    {
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

        public Comment(int lineNumber, string content, MemberDeclarationSyntax methodOrPropertyIfAny, TypeDeclarationSyntax typeIfAny, NamespaceDeclarationSyntax namespaceIfAny)
        {
            if (string.IsNullOrEmpty(content))
                throw new ArgumentException("Null/blank content specified");
            if (lineNumber < 1)
                throw new ArgumentOutOfRangeException("lineNumber");

            LineNumber = lineNumber;
            Name = content;
            MethodOrPropertyIfAny = methodOrPropertyIfAny;
            TypeIfAny = typeIfAny;
            NamespaceIfAny = namespaceIfAny;
        }
    }
}
