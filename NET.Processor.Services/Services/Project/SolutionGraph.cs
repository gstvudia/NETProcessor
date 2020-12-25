using AutoMapper;
using DynamicData;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Configuration;
using NET.Processor.Core.Helpers.Interfaces;
using NET.Processor.Core.Interfaces;
using NET.Processor.Core.Models;
using NET.Processor.Core.Models.API;
using NET.Processor.Core.Models.RelationsGraph.Item;
using NET.Processor.Core.Models.RelationsGraph.Item.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using File = NET.Processor.Core.Models.RelationsGraph.Item.File;

namespace NET.Processor.Core.Services.Project
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

        public IEnumerable<Method> GetRelationsGraph(Solution solution)
        {
            SyntaxNode root = null;
            List<Item> itemRelations = new List<Item>();

            foreach (var project in solution.Projects)
            {
                if (project.Language == _configuration["Framework:Language"])
                {
                    // Get ALL methods inside of each file and save it into methodsRelations
                    foreach (var document in project.Documents)
                    {
                        root = document.GetSyntaxRootAsync().Result;
                        // Add file node 
                        string fileName = Path.GetFileNameWithoutExtension(document.Name);
                        File file = new File(document.Id.Id, fileName);

                        // Adding relations of document (file) and all associated Items
                        itemRelations.Add(file);
                        itemRelations.AddRange(MapItemRelations(root, fileName, document.Id.Id, project.Language));
                    }
                }
            }

            // Filter for method relations
            var methodsRelations = itemRelations
                                   .OfType<Method>()
                                   .Where(x => x.GetType() == typeof(Method))
                                   .ToList();

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

        /// <summary>
        /// Mapping Method, Class, and Namespace relations
        /// </summary>
        /// <param name="root"></param>
        /// <param name="fileName"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        private List<Item> MapItemRelations(SyntaxNode root, string fileName, Guid fileId, string language)
        { 
            List<Method> methodsList = new List<Method>();
            List<Class> classList = new List<Class>();
            List<Namespace> namespaceList = new List<Namespace>();

            var methodNodes = root.DescendantNodes().OfType<MethodDeclarationSyntax>();
            var classDeclarations = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
            var namespaceDeclarations = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>();

            string currentClassName = null;
            ClassDeclarationSyntax currentClass = null;

            //   Save methods and ids in the table, when we load the solution we get the id from the existing
            //      records, later on it will be equaled based on method name and parameters type and amount
            //      it is in method class 
            foreach (var node in methodNodes)
            {
                // Class name 
                currentClass = classDeclarations.FirstOrDefault(
                  c => c.SyntaxTree.GetText().ToString().Contains(node.Identifier.ValueText));

                currentClassName = classDeclarations.FirstOrDefault(
                  c => c.SyntaxTree.GetText().ToString().Contains(node.Identifier.ValueText))
                .Identifier.ValueText;

                // Methods
                // var members = root.DescendantNodes().OfType<MemberDeclarationSyntax>();
                var method = new Method(root.DescendantNodes().IndexOf(node), node.Identifier.ValueText,
                                                            node.Body, fileId, fileName, currentClassName, root.DescendantNodes().IndexOf(currentClass), language);
                
                // Methods relations towards child methods
                if (!methodsList.Any(x => x.Name == method.Name))
                {
                    method.ChildList.AddRange(GetChilds(method));
                    methodsList.Add(method);
                }
            }

            // Adding class relations towards Namespace
            var currentNamespace = namespaceDeclarations.FirstOrDefault();
            var currentNamespaceName = currentNamespace.Name.ToString();
            var containingNamespace = new Namespace(root.DescendantNodes().IndexOf(currentNamespace), 
                currentNamespaceName, fileId, fileName, classList);
            namespaceList.Add(containingNamespace);

            // Adding Method relations towards Class
            Class containingClass = new Class(root.DescendantNodes().IndexOf(currentClass), currentClassName, 
                root.DescendantNodes().IndexOf(currentNamespace), currentNamespaceName, fileId, fileName, methodsList);
            classList.Add(containingClass);

            // Adding all node types to generic item list class
            List<Item> itemList = new List<Item>();
            itemList.AddRange(methodsList);
            itemList.AddRange(classList);
            itemList.Add(namespaceList);

            return itemList;
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
           
                // Pulling information for method from Repository (Github)
                GithubJSONResponse.Root githubJSONResponse = await GetRepositoryInformationForMethod(item.Name, item.FileName, "BennieBe", solutionName);
                nodeBase.nodeData.repositoryCommitLinkOfMethod = githubJSONResponse.items[0]
                    .html_url.Replace("blob", "commits");
                nodeBase.nodeData.repositoryLinkOfMethod = githubJSONResponse.items[0].html_url;
                
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

        private async Task<GithubJSONResponse.Root> GetRepositoryInformationForMethod(string methodName, string fileName, string repositoryOwner, string solutionName)
        {
            // Currently only one option Github, in future it could also come from AzureDevops
            GithubJSONResponse.Root githubJSONResponse = await _githubService.GetLinkToMethod(methodName, fileName, repositoryOwner, solutionName);
            return githubJSONResponse;
        }
    }
}