using NET.Processor.Functions.Services;
using System.Collections.Generic;
using System.IO;

namespace NET.Processor.Functions.Interfaces
{
    public interface ICommentService
    {
        /// <summary>
        /// Finds all references for a comment inside the solution
        /// </summary>
        /// <param name="projects"></param>
        /// <returns>Comments</returns>
        IEnumerable<Comment> GetCommentReferences(IEnumerable<FileInfo> csharpCompileFileList);
    }
}
