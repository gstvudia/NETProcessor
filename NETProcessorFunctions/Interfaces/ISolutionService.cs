using Microsoft.CodeAnalysis;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using NET.Processor.Functions.Models;

namespace NET.Processor.Functions.Interfaces
{
    public interface ISolutionService
    {

        /// <summary>
        /// Loads solution from path
        /// </summary>
        /// <param name="SolutionPath">Path of the .sln file</param>
        /// <returns>Projects</returns>
        Task<Solution> LoadSolution(string path);

        /// <summary>
        /// Loads files from solution path
        /// </summary>
        /// <param name="SolutionPath">Path of the .sln file</param>
        /// <returns>FilePaths</returns>
        //IEnumerable<FileInfo> LoadFilePaths(string SolutionPath);

        List<Item> GetSolutionItems(Solution solution);
    }
}
