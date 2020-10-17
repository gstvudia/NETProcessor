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
using NET.Processor.Core.Helpers;
using NET.Processor.Core.Models.RelationsGraph.Item;

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
            // string path = "CleanArchitecture";
            string path = "TestProject";
            
            // TODO: This Filter should later be served from Filter functionality on the Frontend
            var filter = new Filter();
            //filter.Projects.Add("TestProject");
            //filter.Documents.Add("Program1");
            //filter.Documents.Add("Program2");
            //filter.Methods.Add("Main");
            //filter.Methods.Add("Program1TestFunction1");

            var solution = await _solutionService.LoadSolution(path);
            var listItems =  _solutionService.GetSolutionItems(solution, filter).ToList();

            var methodAndClassesListItems = RelationsGraph.BuildTree(listItems)
                .Where(item => item.GetType().Name == ItemType.Class.ToString() || 
                       item.GetType().Name == ItemType.Method.ToString())
                .ToList();

           List<Node> graphNodes= new List<Node>();
           List<Edge> graphEdges= new List<Edge>();
           NodeData nodeData = new NodeData();
           foreach (var item in methodAndClassesListItems)
           {
                nodeData = _mapper.Map<NodeData>(item);
                nodeData.colorCode = "orange";
                nodeData.weight = 100;
                nodeData.shapeType = "roundrectangle";
                nodeData.nodeType = item.GetType().ToString();
                graphNodes.Add(new Node
                {
                    data = nodeData
                });
           }

           graphEdges = _relationsGraphMapper.MapItemsToEdges(methodAndClassesListItems);

           var relationGraph = new Root
           {
                nodes = graphNodes,
                edges = graphEdges
           };

           return Ok(relationGraph);
        }

        [HttpGet("GetSolution")]
        public async Task<IActionResult> GetSolutionItems()
        {
            // Load complete solution and all assets
            string solutionName = "TestProject";
            var solution = await _solutionService.LoadSolution(solutionName);

            // TODO: This Filter should later be served from Filter functionality on the Frontend
            var filter = new Filter();
            //filter.Projects.Add("TestProject");
            //filter.Documents.Add("Program1");
            //filter.Documents.Add("Program2");
            //filter.Methods.Add("Main");
            //filter.Methods.Add("Program1TestFunction1");

            // Walk through solution nodes and select nodes / assets (project, document) based on filter
            var selectedItems = _solutionService.GetSolutionItems(solution, new Filter()).ToList();
            // Build relationship graph for methods based on filtered / selected nodes
            // var methodsTree = RelationsGraph.BuildTree(selectedItems.Where(item => item.GetType().Name == ItemType.Method.ToString()).ToList());
            // Select specific assets that should be sent back to frontend

            var solutionAssets = new solutionInfo
            {
               // Projects = selectedItems.Where(item => item.GetType().Name == ItemType.Project.ToString()),
               // Documents = selectedItems.Where(item => item.GetType().Name == ItemType.Document.ToString())
            };
 
            return Ok(solutionAssets);
        }

        public struct solutionInfo
        {
            public IEnumerable<string> Projects { get; set; }
            public IEnumerable<string> Documents { get; set; }
        }
    }
}