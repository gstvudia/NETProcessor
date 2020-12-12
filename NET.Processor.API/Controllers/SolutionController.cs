using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using NET.Processor.Core.Models;
using NET.Processor.Core.Interfaces;
using System.Linq;
using Microsoft.CodeAnalysis;
using System;

namespace NET.Processor.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SolutionController : ControllerBase
    {
        private readonly ISolutionService _solutionService;

        public SolutionController(ISolutionService solutionService)
        {
            _solutionService = solutionService;
        }

        /// <summary>
        /// This call processes the solution, it is called automatically via webhook
        /// </summary>
        /// <param name="webHook"></param>
        /// <returns></returns>
        [HttpPost("SaveAndProcessSolutionFromRepository")]
        public async Task<IActionResult> SaveSolutionFromRepository([FromBody] CodeRepository repository)
        {
            _solutionService.SaveSolutionFromRepository(repository);
            await Process(repository.SolutionName, repository.SolutionFilename);
            return Ok("Solution has been processed successfully");
        }

        /// <summary>
        /// This call processes the solution, it is done manually
        /// </summary>
        /// <param name="solutionName"></param>
        /// <returns></returns>
        [HttpPost("ProcessSolution")]
        public async Task<IActionResult> ProcessSolution([FromBody] CodeRepository solution)
        {
            await Process(solution.SolutionName, solution.SolutionFilename);
            return Ok("Solution has been processed successfully");
        }

        /// <summary>
        /// This method loads the solution by solutionName and processes the various items of the solution, 
        /// finally it saves the solution as graph nodes and edges into the database
        /// </summary>
        /// <param name="solutionName"></param>
        /// <returns>solution</returns>
        private async Task<Solution> Process(string solutionName, string solutionFilename)
        {
            var solution = await _solutionService.LoadSolution(solutionName, solutionFilename);
            // If solution path cannot be found, return an error
            if(solution == null)
            {
                throw new Exception("The solution could not be found, have you cloned it into the respective directory before processing the solution?");
            }

            // Process graph nodes and edges and corresponding information (project, file, etc.)
            var relations = _solutionService.GetRelationsGraph(solution).ToList();
            // Store graph nodes and edges and corresponding information (project, file, etc.)
            _solutionService.ProcessRelationsGraph(relations, solutionName);

            return solution;
        }

        /// <summary>
        /// Obsolete since we use client to query database directly
        /// </summary>
        /// <param name="solutionName"></param>
        /// <returns></returns>
        /*
        [HttpGet("GetSolution/{solutionName}")]
        public async Task<IActionResult> GetSolution(string solutionName)
        {
            // TODO: This Filter should later be served from Filter functionality on the Frontend
            var filter = new Filter();
            /* filter.Projects.Add("TestProject");
            filter.Documents.Add("Program1");
            filter.Documents.Add("Program2");
            filter.Methods.Add("Main");
            filter.Methods.Add("Program1TestFunction1");

            // Root relationgraph = await _databaseService.GetCollection(solutionName);
            return Ok(null);
        }
        */

        /// <summary>
        /// Obsolete since we use client to query database directly
        /// </summary>
        /*
        [HttpGet("GetSolutionAssets/{solutionName}")]
        public async Task<IActionResult> GetSolutionAssets(string solutionName)
        {
            
            // Load solution assets
            // var solution = await _solutionService.LoadSolution(solutionName);
            // Walk through solution nodes and select nodes / assets (project, document) based on filter
            var selectedItems = _solutionService.GetSolutionItems(solution, new Filter()).ToList();
            // Build relationship graph for methods based on filtered / selected nodes
            // var methodsTree = RelationsGraph.BuildTree(selectedItems.Where(item => item.GetType().Name == ItemType.Method.ToString()).ToList());
            // Select specific assets that should be sent back to frontend

            var solutionAssets = new SolutionInfo
            {
                Projects = selectedItems
                           .Where(p => p.GetType().Name == ItemType.NodeProject.ToString())
                           .Select(p => p.Name)
                           .Distinct()
                           .ToList(),
                Documents = selectedItems
                        .Where(d => d.GetType().Name == ItemType.NodeDocument.ToString())
                        .Select(d => d.Name)
                        .Distinct()
                        .ToList()
            };
            

            var solutionAssets = null;
            return Ok(solutionAssets);
        }
        */
    }
}
