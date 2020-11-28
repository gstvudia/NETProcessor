﻿using Microsoft.CodeAnalysis;
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
        IEnumerable<Method> GetRelationsGraph(Solution solution);
    }
}
