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
using DynamicData;
using NET.Processor.API.Helpers.Interfaces;
using System.Collections;

namespace NET.Processor.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SolutionController : ControllerBase
    {
        private readonly ISolutionService _solutionService;
        private readonly IMapper _mapper;
        private readonly IRelationsGraphMapper _relationsGraphMapper;

        public SolutionController(ISolutionService solutionService, IMapper mapper,
                                  IRelationsGraphMapper relationsGraphMapper)
        {
            _solutionService = solutionService;
            _mapper = mapper;
            _relationsGraphMapper = relationsGraphMapper;
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
            // string path = @"" + homeDrive + homePath + "\\source\\repos\\Solutions\\CleanArchitecture-master\\CleanArchitecture.sln";
            string path = @"" + homeDrive + homePath + "\\source\\repos\\NETWebhookTest\\TestProject.sln";
            
            // TODO: This Filter should later be served from Filter functionality on the Frontend
            var filter = new Filter();
            //filter.Projects.Add("TestProject");
            //filter.Documents.Add("Program1");
            //filter.Documents.Add("Program2");
            //filter.Methods.Add("Main");
            //filter.Methods.Add("Program1TestFunction1");

            var solution = await _solutionService.LoadSolution(path);
            var listItems =  _solutionService.GetSolutionItems(solution, filter).ToList();

           List<Node> graphNodes= new List<Node>();
           List<Edge> graphEdges= new List<Edge>();
           NodeData nodeData = new NodeData();
           foreach (var item in listItems)
           {
                nodeData = _mapper.Map<NodeData>(item);
                nodeData.colorCode = "orange";
                nodeData.weight = 100;
                nodeData.shapeType = "roundrectangle";
                graphNodes.Add(new Node
                {
                    data = nodeData
                });
           }

           graphEdges = _relationsGraphMapper.MapItemsToEdges(listItems);

           var relationGraph = new Root
           {
                nodes = graphNodes,
                edges = graphEdges
           };

           return Ok(relationGraph);
        }

        [HttpGet("GetSolution")]
        public async Task<IActionResult> GetSolutionItems([FromQuery] string solutionName)
        {
            var homeDrive = Environment.GetEnvironmentVariable("HOMEDRIVE");
            var homePath = Environment.GetEnvironmentVariable("HOMEPATH");
            string path = string.Empty;

            switch (solutionName)
            {
                case "CleanArchitecture":
                    path = @"" + homeDrive + homePath + "\\source\\repos\\Solutions\\CleanArchitecture-master\\CleanArchitecture.sln";
                    break;
                case "TestProject":
                    path = @"" + homeDrive + homePath + "\\source\\repos\\NETWebhookTest\\TestProject.sln";
                    break;
            }            
            
            var solution = await _solutionService.LoadSolution(path);

            var ret = new solutionInfo()
            {
                projects = solution.Projects.Select(p => p.Name)
            };

            var classes = solution.Projects.Select(p => p.Documents);

            ret.classes = classes.Select(c => c.Select(c => c.Name)).FirstOrDefault();
            ret.methods = _solutionService.GetSolutionItems(solution, new Filter()).Select(m => m.Name);

            return Ok(ret);
        }

        public struct solutionInfo
        {
            public IEnumerable<string> projects { get; set; }
            public IEnumerable<string> classes { get; set; }
            public IEnumerable<string> methods { get; set; }
        }
    }
}