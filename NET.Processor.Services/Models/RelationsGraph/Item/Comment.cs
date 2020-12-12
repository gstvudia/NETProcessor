using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace NET.Processor.Core.Models.RelationsGraph.Item
{
    public class Comment : Item
    {
        /// <summary>
        /// This will always be a positive integer
        /// </summary>
        [BsonElement("LineNumber")]
        public int LineNumber { get; private set; }

        /// <summary>
        ///  This is the id of the source (method, class, field etc.) that this comment is attached to
        /// </summary>
        [BsonElement("AttachedPropertyId")]
        public int AttachedPropertyId { get; private set; }

        /// <summary>
        ///  This is the name of the source (method, class, field etc.) that this comment is attached to
        /// </summary>
        [BsonElement("AttachedPropertyName")]
        public string AttachedPropertyName { get; private set; }

        /// <summary>
        /// This may be null since the comment may not exist within a method or property
        /// </summary>
        [BsonIgnore]
        public MemberDeclarationSyntax MethodOrPropertyIfAny { get; private set; }

        /// <summary>
        /// This may be null since the comment may not exist within an class, interface or struct
        /// </summary>
        [BsonIgnore]
        public TypeDeclarationSyntax TypeIfAny { get; private set; }

        /// <summary>
        /// This may be null since the comment may not exist within a namespace
        /// </summary>
        [BsonIgnore]
        public NamespaceDeclarationSyntax NamespaceIfAny { get; private set; }

        public Comment(int lineNumber, string content, int attachedPropertyId,
                       string attachedPropertyName, MemberDeclarationSyntax methodOrPropertyIfAny,
                       TypeDeclarationSyntax typeIfAny, NamespaceDeclarationSyntax namespaceIfAny)                                              
        {
            if (string.IsNullOrEmpty(content))
                throw new ArgumentException("Null/blank content specified");
            if (lineNumber < 1)
                throw new ArgumentOutOfRangeException("lineNumber");

            LineNumber = lineNumber;
            Name = content;
            AttachedPropertyId = attachedPropertyId;
            AttachedPropertyName = attachedPropertyName;
            MethodOrPropertyIfAny = methodOrPropertyIfAny;
            TypeIfAny = typeIfAny;
            NamespaceIfAny = namespaceIfAny;
        }
    }
}
