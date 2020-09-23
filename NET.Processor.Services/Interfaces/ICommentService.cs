﻿using NET.Processor.Core.Services;
using System.Collections.Generic;
using System.IO;

namespace NET.Processor.Core.Interfaces
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