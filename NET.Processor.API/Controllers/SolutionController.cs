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
using NET.Processor.Core.Services;
using AutoMapper;
using System.Linq;
using NET.Processor.API.Models.DTO;
using NET.Processor.API.Helpers.Mappers;

namespace NET.Processor.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SolutionController : ControllerBase
    {
        private readonly ISolutionService _solutionService;
        private readonly IMapper _mapper;

        public SolutionController(ISolutionService solutionService, IMapper mapper)
        {
            _solutionService = solutionService;
            _mapper = mapper;

        }

        [HttpGet("test")]
        public bool Get()
        {
            return true;
        }

        [HttpPost("ProcessSolution")]
        public async Task<IActionResult> ProcessSolution([FromBody] WebHook webHook)
        {
            var homeDrive = Environment.GetEnvironmentVariable("HOMEDRIVE");
            var homePath = Environment.GetEnvironmentVariable("HOMEPATH");
            string path = @"" + homeDrive + homePath + "\\source\\repos\\Solutions\\CleanArchitecture-master\\CleanArchitecture.sln";

            var solution = await _solutionService.LoadSolution(path);
            var listItems =  _solutionService.GetSolutionItems(solution).ToList();

            //THE MAPPING PROBLEM IS NOT RELATED TO CONFIGURATION, THER EIS SOMETHING IM DOING WRONG ON THE C
            Root relationGraph = new Root();
           //List<Node> graphNodes= new List<Node>();
           //foreach (var item in listItems)
           //{
           //    graphNodes.Add( new Node (
           //                        _mapper.Map<Data>(item)
           //                     ));
           //}
           //relationGraph.nodes.Add(graphNodes);
            return Ok(relationGraph);
        }

        [HttpGet("GetSolutionItems")]
        public async Task<IActionResult> GetSolutionItems()
        {

            string path = Directory.GetParent(Directory.GetCurrentDirectory()).FullName + "NET.Processor.Services/bin/Debug\netcoreapp3.1/Solutions/CleanArchitecture-master/CleanArchitecture.sln";
            //string path = @"C:\Users\Gustavo Melo\source\repos\NETProcessor\NET.Processor.Services\bin\Debug\netcoreapp3.1\Solutions\CleanArchitecture-master\CleanArchitecture.sln";
            var solution = await _solutionService.LoadSolution(path);
            //var solutionItens = await
            //_solutionService.GetSolutionItens(solution);
          
            //var itensToReturn = _mapper.Map<IEnumerable<UserForListDTO>>(itens);

            return Ok();
        }
    }
}