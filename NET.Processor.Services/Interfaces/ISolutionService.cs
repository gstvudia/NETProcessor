using Microsoft.CodeAnalysis;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using NET.Processor.Core.Models;

namespace NET.Processor.Core.Interfaces
{
    public interface ISolutionService
    {
        /// <summary>
        /// Loads solution from path
        /// </summary>
        /// <param name="SolutionPath">Path of the .sln file</param>
        /// <returns>Projects</returns>
        Solution LoadSolution(string SolutionPath);

        /// <summary>
        /// Loads files from solution path
        /// </summary>
        /// <param name="SolutionPath">Path of the .sln file</param>
        /// <returns>FilePaths</returns>
        //IEnumerable<FileInfo> LoadFilePaths(string SolutionPath);

        void GetSolutionItens(Solution solution);

        Task<bool> GetSolutionFromRepo(WebHook webHook);
    }
}
