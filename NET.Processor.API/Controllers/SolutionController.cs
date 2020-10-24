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
using MongoDB.Driver.Core.Operations;
using NET.Processor.Core.Services.Database;

namespace NET.Processor.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SolutionController : ControllerBase
    {
        private readonly ISolutionService _solutionService;
        private readonly IDatabaseService _databaseService;
        private readonly IMapper _mapper;
        private readonly IRelationsGraphMapper _relationsGraphMapper;

        public SolutionController(ISolutionService solutionService, IDatabaseService 
                                  databaseService, IMapper mapper, IRelationsGraphMapper relationsGraphMapper)
        {
            _solutionService = solutionService;
            _databaseService = databaseService;
            _databaseService.ConnectDatabase();
            _mapper = mapper;
            _relationsGraphMapper = relationsGraphMapper;
        }

        [HttpGet("test")]
        public bool Get()
        {
            return true;
        }

        // This call is being made automatically through a Webhook
        [HttpPost("ProcessSolution/Webhook")]
        public async Task<IActionResult> ProcessSolution([FromBody] WebHook webHook)
        {
            return Ok();
        }

        // This call is being made manually through triggering on the platform
        [HttpGet("ProcessSolution/{solutionName}")]
        public async Task<IActionResult> ProcessSolution(string solutionName)
        {
            var solution = await _solutionService.LoadSolution(solutionName);
            var listItems =  _solutionService.GetSolutionItems(solution).ToList();

            // Store collection in Database
            _databaseService.StoreCollection(solutionName, listItems);
            // Remark: No need to close db again, handled by database engine (MongoDB)

           return Ok("Solution has been processed successfully");
        }

        [HttpGet("GetSolution/{solutionName}")]
        public IEnumerable<Item> GetSolution(string solutionName)
        {
            // TODO: This Filter should later be served from Filter functionality on the Frontend
            var filter = new Filter();
            /* filter.Projects.Add("TestProject");
            filter.Documents.Add("Program1");
            filter.Documents.Add("Program2");
            filter.Methods.Add("Main");
            filter.Methods.Add("Program1TestFunction1");*/

            IEnumerable<Item> listItems = _databaseService.GetCollection(solutionName);

            listItems = RelationsGraph.BuildTree(listItems.ToList())
                .Where(item => item.GetType().Name == ItemType.Class.ToString() ||
                       item.GetType().Name == ItemType.Method.ToString()) //||
                                                                          //item.GetType().Name == ItemType.Comment.ToString() ||
                                                                          //item.GetType().Name == ItemType.Namespace.ToString())
            .ToList();

            List<Node> graphNodes = new List<Node>();
            List<Edge> graphEdges = new List<Edge>();
            NodeData nodeData = new NodeData();
            foreach (var item in listItems)
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

            graphEdges = _relationsGraphMapper.MapItemsToEdges(listItems.ToList());

            var relationGraph = new Root
            {
                nodes = graphNodes,
                edges = graphEdges
            };

            return listItems;
        }

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

            var solutionAssets = new solutionInfo
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

        public struct solutionInfo
        {
            public IEnumerable<string> Projects { get; set; }
            public IEnumerable<string> Documents { get; set; }
        }
    }
}