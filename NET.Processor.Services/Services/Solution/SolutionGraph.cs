using AutoMapper;
using DynamicData;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Configuration;
using NET.Processor.Core.Helpers.Interfaces;
using NET.Processor.Core.Interfaces;
using NET.Processor.Core.Models;
using NET.Processor.Core.Models.API;
using NET.Processor.Core.Models.RelationsGraph.Item.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using VSSolution = Microsoft.CodeAnalysis.Solution;

namespace NET.Processor.Core.Services.Solution
{
    public class SolutionGraph : ISolutionGraph
    {
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IRelationsGraphMapper _relationsGraphMapper;
        private readonly IGithubService _githubService;

        public SolutionGraph(IMapper mapper, IConfiguration configuration, IRelationsGraphMapper relationsGraphMapper, IGithubService githubService)
        {
            _githubService = githubService;
            _mapper = mapper;
            _configuration = configuration;
            _relationsGraphMapper = relationsGraphMapper;
        }

        public IEnumerable<Method> GetRelationsGraph(VSSolution solution)
        {
            SyntaxNode root = null;
            List<Method> methodsRelations = new List<Method>();

            foreach (var project in solution.Projects)
            {
                if (project.Language == _configuration["Framework:Language"])
                {
                    // Get ALL methods inside of each file and save it into methodsRelations
                    foreach (var document in project.Documents)
                    {
                        root = document.GetSyntaxRootAsync().Result;
                        methodsRelations.AddRange(MapMethods(root, Path.GetFileNameWithoutExtension(document.Name), project.Language));
                    }
                }
            }

            // Set Ids for each child for being able to reference them later on edges (relations between nodes)
            foreach (var method in methodsRelations)
            {
                // After all methods are mapped, we set the respective ids from the list
                // Remove methods that should not be in the graph like toString() by setting the child.id to 0
                // If it is not in our methods list, it is being removed and set to child.id 0
                // 
                // TODO: Remark: There might be methods that we need to keep, such as Async Methods that would
                // not show up if we remove them from the Graph, instead of removing the methods we should tag
                // them as third party, but still remove stuff like toString() by for example creating a custom filter
                // filtering out those methods by namespace or library
                foreach (var child in method.ChildList.Where(x => x.Id == -1).ToList())
                {
                    child.Id = methodsRelations.Where(x => x.Name == child.Name).Select(x => x.Id).FirstOrDefault();
                }
            }

            // Remove built or invalid methods, this is where all methods with id 0 are removed
            foreach (var method in methodsRelations)
            {
                method.ChildList.RemoveAll(c => c.Id == 0 || c.Name == string.Empty);
            }
            return methodsRelations;
        }

        private List<Method> MapMethods(SyntaxNode root, string fileName, string language)
        { 
        //   Save methods and ids in the table, when we load the solution we get the id from the existing
        //      records, later on it will be equaled based on method name and parameters type and amount
        //      it is in method class 
            List<Method> methodsList = new List<Method>();
            var methodNodes = root.DescendantNodes().OfType<MethodDeclarationSyntax>();
            var classDeclarations = root.DescendantNodes().OfType<ClassDeclarationSyntax>();

            foreach (var node in methodNodes)
            {
                var members = root.DescendantNodes().OfType<MemberDeclarationSyntax>();
                var className = classDeclarations.FirstOrDefault(
                                c => c.SyntaxTree.GetText().ToString().Contains(node.Identifier.ValueText))
                            .Identifier.ValueText;
                var method = new Method(root.DescendantNodes().IndexOf(node), node.Identifier.ValueText,
                                                            node.Body, fileName, className, language);
                
                if (!methodsList.Any(x => x.Name == method.Name))
                {
                    method.ChildList.AddRange(GetChilds(method));
                    methodsList.Add(method);
                }
            }

            return methodsList;
        }

        private List<Method> GetChilds(Method method)
        {
            List<Method> childList = new List<Method>();

            if (method.Body != null)
            {
                var invokees = method.Body.Statements.Where(x => x.Kind().ToString() == "ExpressionStatement").ToList();
                foreach (var invoked in invokees)
                {
                    var expression = invoked as ExpressionStatementSyntax;
                    var arguments = expression.Expression as InvocationExpressionSyntax;
                    string[] child = null;

                    child = invoked.ToString().Replace(";", string.Empty).Split(".");
                    
                    if (child.Length > 1)
                    {
                        childList
                            .Add(new Method(
                                -1, child[1].Substring(0, child[1].LastIndexOf("(") + 1).Replace("(", string.Empty)));
                    }
                    else
                    {
                        childList
                            .Add(new Method(
                                -1, child[0].Substring(0, child[0].LastIndexOf("(") + 1).Replace("(", string.Empty)));
                    }
                }
            }

            return childList;
        }

        public async Task<ProjectRelationsGraph> ProcessRelationsGraph(IEnumerable<Method> relations, string solutionName, string repositoryToken)
        {
            List<Node> graphNodes = new List<Node>();

            foreach (var item in relations)
            {
                NodeRoot nodeBase = _mapper.Map<NodeRoot>(item);
                nodeBase.colorCode = "orange";
                nodeBase.weight = 100;
                nodeBase.shapeType = "roundrectangle";
                nodeBase.nodeType = item.GetType().ToString();
                nodeBase.nodeData.name = item.Name;
                nodeBase.nodeData.fileName = item.FileName;
                nodeBase.nodeData.className = item.ClassName;
                nodeBase.nodeData.language = item.Language;
                nodeBase.nodeData.repositoryLinkToMethod = await getRepositoryLinkToMethod(item.Name, item.FileName, "BennieBe", solutionName);
                graphNodes.Add(new Node
                {
                    data = nodeBase
                });
            }

            List<Edge> graphEdges = _relationsGraphMapper.MapItemsToEdges(relations.ToList());

            var relationGraph = new ProjectRelationsGraph();
            relationGraph.Id = MongoDB.Bson.ObjectId.GenerateNewId();
            relationGraph.SolutionName = solutionName;
            relationGraph.graphData.nodes = graphNodes;
            relationGraph.graphData.edges = graphEdges;

            return relationGraph;
        }

        private async Task<string> getRepositoryLinkToMethod(string methodName, string fileName, string repositoryOwner, string solutionName)
        {
            // Currently only one option Github, in future it could also come from AzureDevops
            GithubJSONResponse.Root githubJSONResponse = await _githubService.GetLinkToMethod(methodName, fileName, repositoryOwner, solutionName);
            return githubJSONResponse.items[0].html_url;
        }
    }
}