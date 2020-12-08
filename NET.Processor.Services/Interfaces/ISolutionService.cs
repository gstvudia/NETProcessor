using Microsoft.CodeAnalysis;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using NET.Processor.Core.Models;
using NET.Processor.Core.Services;
using NET.Processor.Core.Models.RelationsGraph.Item;
using System;

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
        /// Loads files from solution path
        /// </summary>
        /// <param name="SolutionPath">Path of the .sln file</param>
        /// <returns>FilePaths</returns>
        //IEnumerable<FileInfo> LoadFilePaths(string SolutionPath);
        IEnumerable<Item> GetSolutionItems(Solution solution);
        /// <summary>
        /// Save Items of solution in the database
        /// </summary>
        /// <param name="listItems"></param>
        void SaveSolutionItems(List<Item> listItems, string solutionName);
        /// <summary>
        /// Getting all method nodes and relations
        /// </summary>
        /// <param name="solution"></param>
        /// <returns></returns>
        IEnumerable<Method> GetRelationsGraph(Solution solution);
        /// <summary>
        /// Processing method nodes and relations
        /// </summary>
        /// <param name="relations"></param>
        void ProcessRelationsGraph(IEnumerable<Method> relations, string solutionName);
    }
}
