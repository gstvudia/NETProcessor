using Microsoft.CodeAnalysis;
using System.Threading.Tasks;

namespace NET.Processor.Core.Interfaces
{
    interface ISolutionService
    {
        /// <summary>
        /// Loads solution from path
        /// </summary>
        /// <param name="SolutionPath">Path of the .sln file</param>
        /// <returns>Projects</returns>
        Task<Solution> LoadSolution(string SolutionPath);
    }
}
