using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using NET.Processor.Core.Models;
using NET.Processor.Core.Interfaces;
using AutoMapper;
using System.Linq;
using NET.Processor.Core.Models.RelationsGraph.Item.Base;
using NET.Processor.Core.Helpers.Interfaces;
using NET.Processor.Core.Helpers;

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
            listItems = RelationsGraph.BuildTree(listItems.Where(item => item.GetType().Name == ItemType.Method.ToString()).ToList());

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

            // Store collection in Database
            _databaseService.StoreCollection(solutionName, relationGraph);
            // Remark: No need to close db again, handled by database engine (MongoDB)

           return Ok("Solution has been processed successfully");
        }

        [HttpGet("GetSolution/{solutionName}")]
        public async Task<IActionResult> GetSolution(string solutionName)
        {
            // TODO: This Filter should later be served from Filter functionality on the Frontend
            var filter = new Filter();
            /* filter.Projects.Add("TestProject");
            filter.Documents.Add("Program1");
            filter.Documents.Add("Program2");
            filter.Methods.Add("Main");
            filter.Methods.Add("Program1TestFunction1");*/

            // Root relationgraph = await _databaseService.GetCollection(solutionName);
            return Ok(null);
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