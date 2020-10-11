﻿using Microsoft.AspNetCore.Authorization;
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

namespace NET.Processor.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SolutionController : ControllerBase
    {
        //MOVE THE REPOSITORY TO THE CORE PROJECT
        private readonly ISolutionService _solutionService;
        private readonly ICommentService _commentService;

        public SolutionController(ISolutionService solutionService, ICommentService commentService)
        {
            _solutionService = solutionService;
            _commentService = commentService;
        }

        [HttpGet("test")]
        public async Task<bool> Get()
        {
            return true;
        }

        [HttpPost("ProcessSolution")]
        public async Task<IActionResult> ProcessSolution([FromBody] WebHook webHook)
        {
            List<string> list = null;
            var homeDrive = Environment.GetEnvironmentVariable("HOMEDRIVE");
            var homePath = Environment.GetEnvironmentVariable("HOMEPATH");
            // string path = @"" + homeDrive + homePath + "\\source\\repos\\Solutions\\CleanArchitecture-master\\CleanArchitecture.sln";
            string path = @"" + homeDrive + homePath + "\\source\\repos\\TestProject\\TestProject\\TestProject.sln";
            var solution = await _solutionService.LoadSolution(path, _commentService);
            var test =  _solutionService.GetSolutionItems(solution);

            //var itensToReturn = _mapper.Map<IEnumerable<UserForListDTO>>(itens);

            return Ok(test);
        }

        [HttpGet("GetSolutionItems")]
        public async Task<IActionResult> GetSolutionItems()
        {

            string path = Directory.GetParent(Directory.GetCurrentDirectory()).FullName + "NET.Processor.Services/bin/Debug\netcoreapp3.1/Solutions/CleanArchitecture-master/CleanArchitecture.sln";
            //string path = @"C:\Users\Gustavo Melo\source\repos\NETProcessor\NET.Processor.Services\bin\Debug\netcoreapp3.1\Solutions\CleanArchitecture-master\CleanArchitecture.sln";
            var solution = _solutionService.LoadSolution(path, _commentService);
            //var solutionItens = await
            //_solutionService.GetSolutionItens(solution);
          
            //var itensToReturn = _mapper.Map<IEnumerable<UserForListDTO>>(itens);

            return Ok();
        }
    }
}