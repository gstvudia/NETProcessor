using AutoMapper;
using NETProcessor.API.Data;
using NETProcessor.API.Data.DTO;
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

namespace NETProcessor.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SolutionController : ControllerBase
    {
        //MOVE THE REPOSITORY TO THE CORE PROJECT
        private readonly ISolutionRepository _solutionRepository;
        private readonly ISolutionService _solutionService;

        public SolutionController(ISolutionRepository solutionRepository, ISolutionService solutionService)
        {
            _solutionRepository = solutionRepository;
            _solutionService = solutionService;
        }

        [HttpGet]
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

        [HttpGet("GetSolutionItens")]
        public async Task<IActionResult> GetSolutionItens()
        {
            
            string path = Directory.GetParent(Directory.GetCurrentDirectory()).FullName + @"\NET.Processor.Services\Solutions\CleanArchitecture-master/CleanArchitecture.sln";
            var solution = _solutionService.LoadSolution(path);
            //var solutionItens = await
                _solutionService.GetSolutionItens(solution);
            //var itens = await _solutionService.GetSolutionItens(solution);
            //var itensToReturn = _mapper.Map<IEnumerable<UserForListDTO>>(itens);

            return Ok("works");
        }
    }
}
