using Microsoft.CodeAnalysis;
using NET.Processor.Core.Models.RelationsGraph.Item;
using NET.Processor.Core.Services;
using System.Collections.Generic;
using System.IO;

namespace NET.Processor.Core.Interfaces
{
    public interface ICommentService
    {
        /// <summary>
        /// Finds all references for a comment inside the solution, it assigns the comment to specific property ids from KeyValuePair itemNames 
        /// </summary>
        /// <param name="rootNode"></param>
        /// <param name="itemNames"></param>
        /// <returns>Comment</returns>
        // IEnumerable<Comment> GetCommentReferences(IEnumerable<FileInfo> csharpCompileFileList);
        IEnumerable<Comment> GetCommentReferences(SyntaxNode rootNode, IEnumerable<KeyValuePair<string, int>> itemNames);
    }
}
