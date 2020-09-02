using Microsoft.CodeAnalysis;

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
    }
}
