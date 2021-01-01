using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NET.Processor.Core.Helpers;
using NET.Processor.Core.Interfaces;
using NET.Processor.Core.Models.RelationsGraph.Item;
using System;
using System.Collections.Generic;
using System.IO;

namespace NET.Processor.Core.Services
{
    public class CommentService : ICommentService
    {
		CommentIdentifier _commentIdentifier = null;
		private readonly IHostEnvironment _env = null;

		public CommentService(IHostEnvironment env)
        {
			_commentIdentifier = new CommentIdentifier();
			_env = env;
		}

        public IEnumerable<Comment> GetCommentReferences(SyntaxNode rootNode, IEnumerable<KeyValuePair<string, string>> itemNames)
        {
			List<Comment> comments = new List<Comment>();
			// Check if an empty key value pair has been passed
			foreach(var itemName in itemNames)
            {
				if (itemName.Equals(default(KeyValuePair<string, string>)))
				{
					throw new Exception("KeyValuePair<string, string> itemNames passed to Comment Service may not be empty!");
				}
			}

			foreach (var comment in _commentIdentifier.GetComments(rootNode.SyntaxTree.GetRoot(), itemNames))
			{
				// Print additional console information in dev mode
				if (_env.IsDevelopment())
					PrintComment(comment);

				// Adding comment to comment list
				comments.Add(comment);
			}
			return comments;
		}

		void PrintComment(Comment comment)
        {
			Console.WriteLine(comment.Name);
			Console.WriteLine();
			if (comment.NamespaceIfAny == null)
				Console.WriteLine("Not in any namespace");
			else
			{
				Console.WriteLine("Namespace: " + comment.NamespaceIfAny.Name);
				if (comment.TypeIfAny != null)
					Console.WriteLine("Type: " + comment.TypeIfAny.Identifier);
				if (comment.MethodOrPropertyIfAny != null)
				{
					Console.Write("Method/Property: ");
					if (comment.MethodOrPropertyIfAny is ConstructorDeclarationSyntax)
						Console.Write(".ctor");
					else if (comment.MethodOrPropertyIfAny is MethodDeclarationSyntax)
						Console.Write(((MethodDeclarationSyntax)comment.MethodOrPropertyIfAny).Identifier);
					else if (comment.MethodOrPropertyIfAny is IndexerDeclarationSyntax)
						Console.Write("[Indexer]");
					else if (comment.MethodOrPropertyIfAny is PropertyDeclarationSyntax)
						Console.Write(((PropertyDeclarationSyntax)comment.MethodOrPropertyIfAny).Identifier);
					else
						Console.Write("?");
					Console.WriteLine();
				}
			}
		}
	}
}
