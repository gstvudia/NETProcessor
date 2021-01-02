using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Threading.Tasks;
using NET.Processor.Core.Models;
using NET.Processor.Core.Models.RelationsGraph.Item;

namespace NET.Processor.Core.Interfaces
{
    public interface ISolutionService
    {
        /// <summary>
        /// Loads solution from path
        /// </summary>
        /// <param name="SolutionPath">Path of the .sln file</param>
        /// <returns>Projects</returns>
        Task<Solution> LoadSolution(string solutionName, string solutionFilename);

        /// <summary>
        /// Load solution from repository
        /// </summary>
        /// <param name="repositoryName"></param>
        /// <returns></returns>
        void SaveSolutionFromRepository(CodeRepository repository);

        /// <summary>
        /// Getting all method nodes and relations
        /// </summary>
        /// <param name="solution"></param>
        /// <returns></returns>
        IEnumerable<Item> GetRelationsGraph(Solution solution);
        /// <summary>
        /// Processing method nodes and relations
        /// </summary>
        /// <param name="relations"></param>
        void ProcessRelationsGraph(IEnumerable<Item> relations, string solutionName, string repositoryToken);
    }
}
