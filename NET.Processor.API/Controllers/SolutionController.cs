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
            await Process(repository.SolutionName, repository.SolutionFilename, repository.Token);
            return Ok("Solution has been processed successfully");
        }

        /// <summary>
        /// This call processes the solution, it is done manually
        /// </summary>
        /// <param name="solutionName"></param>
        /// <returns></returns>
        [HttpPost("ProcessSolution")]
        public async Task<IActionResult> ProcessSolution([FromBody] CodeRepository repository)
        {
            await Process(repository.SolutionName, repository.SolutionFilename, repository.Token);
            return Ok("Solution has been processed successfully");
        }

        /// <summary>
        /// This method loads the solution by solutionName and processes the various items of the solution, 
        /// finally it saves the solution as graph nodes and edges into the database
        /// </summary>
        /// <param name="solutionName"></param>
        /// <returns>solution</returns>
        private async Task<Solution> Process(string solutionName, string solutionFilename, string repositoryToken)
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
            _solutionService.ProcessRelationsGraph(relations, solutionName, repositoryToken);

            return solution;
        }
    }
}
