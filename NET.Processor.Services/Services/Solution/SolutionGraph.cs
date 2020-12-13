using AutoMapper;
using DynamicData;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Configuration;
using NET.Processor.Core.Helpers.Interfaces;
using NET.Processor.Core.Models;
using NET.Processor.Core.Models.RelationsGraph.Item.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VSSolution = Microsoft.CodeAnalysis.Solution;

namespace NET.Processor.Core.Services.Solution
{
    public class SolutionGraph : ISolutionGraph
    {
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IRelationsGraphMapper _relationsGraphMapper;

        public SolutionGraph(IMapper mapper, IConfiguration configuration, IRelationsGraphMapper relationsGraphMapper)
        {
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
                    foreach (var document in project.Documents)
                    {
                        root = document.GetSyntaxRootAsync().Result;
                        methodsRelations.AddRange(MapMethods(root, Path.GetFileNameWithoutExtension(document.Name), project.Language));
                    }
                }
            }

            //Map childs id
            foreach (var method in methodsRelations)
            {

                foreach (var child in method.ChildList.Where(x => x.Id == -1).ToList())
                {
                    child.Id = methodsRelations.Where(x => x.Name == child.Name).Select(x => x.Id).FirstOrDefault();
                }

            }

            //Remove built or invalid methods
            foreach (var method in methodsRelations)
            {
                method.ChildList.RemoveAll(c => c.Id == 0 || c.Name == string.Empty);
            }
            return methodsRelations;
        }

        private List<Method> MapMethods(SyntaxNode root, string fileName, string language)
        {
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

        public ProjectRelationsGraph ProcessRelationsGraph(IEnumerable<Method> relations, string solutionName)
        {
            List<Node> graphNodes = new List<Node>();
            List<Edge> graphEdges = new List<Edge>();
            NodeRoot nodeBase = new NodeRoot();

            foreach (var item in relations)
            {
                nodeBase = _mapper.Map<NodeRoot>(item);
                nodeBase.ColorCode = "orange";
                nodeBase.Weight = 100;
                nodeBase.ShapeType = "roundrectangle";
                nodeBase.NodeType = item.GetType().ToString();
                nodeBase.NodeData.Name = item.Name;
                nodeBase.NodeData.FileName = item.FileName;
                nodeBase.NodeData.ClassName = item.ClassName;
                nodeBase.NodeData.Language = item.Language;
                graphNodes.Add(new Node
                {
                    Data = nodeBase
                });
            }

            graphEdges = _relationsGraphMapper.MapItemsToEdges(relations.ToList());

            var relationGraph = new ProjectRelationsGraph();
            relationGraph.Id = MongoDB.Bson.ObjectId.GenerateNewId();
            relationGraph.SolutionName = solutionName;
            relationGraph.graphData.Nodes = graphNodes;
            relationGraph.graphData.Edges = graphEdges;

            return relationGraph;
        }
    }
}
