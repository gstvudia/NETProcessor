using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using NET.Processor.Core.Models;
using Microsoft.CodeAnalysis;
using NET.Processor.Core.Interfaces;
using System.IO;


namespace NET.Processor.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SolutionController : ControllerBase
    {
        //MOVE THE REPOSITORY TO THE CORE PROJECT
        private readonly ISolutionService _solutionService;

        public SolutionController(ISolutionService solutionService)
        {
            _solutionService = solutionService;
        }

        [HttpGet("test")]
        public async Task<bool> Get()
        {
            return true;
        }

        [HttpPost("ProcessSolution")]
        public async Task<IActionResult> ProcessSolution([FromBody] WebHook webHook)
        {
            var solution = await _solutionService.GetSolutionFromRepo(webHook);
            //var itens = await _solutionService.GetSolutionItens(solution);
            //var itensToReturn = _mapper.Map<IEnumerable<UserForListDTO>>(itens);

            return Ok();
        }

        [HttpGet("GetSolutionItems")]
        public async Task<IActionResult> GetSolutionItems()
        {

             string path = Directory.GetParent(Directory.GetCurrentDirectory()).FullName + "NET.Processor.Services/bin/Debug\netcoreapp3.1/Solutions/CleanArchitecture-master/CleanArchitecture.sln";
            //string path = @"C:\Users\Gustavo Melo\source\repos\NETProcessor\NET.Processor.Services\bin\Debug\netcoreapp3.1\Solutions\CleanArchitecture-master\CleanArchitecture.sln";
            var solution = _solutionService.LoadSolution(path);
            //var solutionItens = await
            //_solutionService.GetSolutionItens(solution);
          
            //var itensToReturn = _mapper.Map<IEnumerable<UserForListDTO>>(itens);

            return Ok(_solutionService.GetSolutionItems(solution));
        }
    }
}