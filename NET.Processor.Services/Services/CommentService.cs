using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NET.Processor.Core.Interfaces;
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

        public IEnumerable<Comment> GetCommentReferences(SyntaxNode rootNode)
        {
			List<Comment> comments = new List<Comment>();

			foreach (var comment in _commentIdentifier.GetComments(rootNode.SyntaxTree.GetRoot()))
			{
				// Print additional console information in dev mode
				if (_env.IsDevelopment())
					PrintComment(comment);

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
					// Print additional console information in dev mode
					if (_env.IsDevelopment())
						PrintComment(comment);

					// Adding comment to comment list
					comments.Add(comment);
				}
			}

			return comments;
		}

		void PrintComment(Comment comment)
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
		}
	}
}
