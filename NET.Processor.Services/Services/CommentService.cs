using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NET.Processor.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;

namespace NET.Processor.Core.Services
{
    public class CommentService : ICommentService
    {

        public CommentService()
        {

        }

        public IEnumerable<Comment> GetCommentReferences(SyntaxNode rootNode)
        {
			List<Comment> comments = new List<Comment>();
			var commentIdentifier = new CommentIdentifier();

			foreach (var comment in commentIdentifier.GetComments(rootNode.SyntaxTree.GetRoot()))
			{
				Console.WriteLine(comment.Content);
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
				Console.WriteLine(rootNode.FullSpan + ":" + comment.LineNumber);
				Console.WriteLine();

				// Adding comment to comment list
				comments.Add(comment);
			}
			return comments;
		}

        IEnumerable<Comment> ICommentService.GetCommentReferences(IEnumerable<FileInfo> csharpCompileFileList)
		{
			List<Comment> comments = new List<Comment>();
			var commentIdentifier = new CommentIdentifier();

			foreach (var csharpCompileFile in csharpCompileFileList)
            {
				foreach (var comment in commentIdentifier.GetComments(csharpCompileFile.OpenText().ReadToEnd()))
				{
					Console.WriteLine(comment.Content);
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
					Console.WriteLine(csharpCompileFile.FullName + ":" + comment.LineNumber);
					Console.WriteLine();

					// Adding comment to comment list
					comments.Add(comment);
				}
			}

			Console.WriteLine("Success! Press [Enter] to continue..");
			Console.ReadLine();

			return comments;
		}
	}
}
