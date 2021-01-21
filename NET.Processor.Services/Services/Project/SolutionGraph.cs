using System;
using AutoMapper;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using NET.Processor.Core.Helpers.Interfaces;
using NET.Processor.Core.Interfaces;
using NET.Processor.Core.Models.API;
using NET.Processor.Core.Models.RelationsGraph.Item;
using NET.Processor.Core.Models.RelationsGraph.Item.Base;
using Interface = NET.Processor.Core.Models.RelationsGraph.Item.Interface;
using File = NET.Processor.Core.Models.RelationsGraph.Item.File;
using NETProcessorProject = NET.Processor.Core.Models.RelationsGraph.Item.Project;

namespace NET.Processor.Core.Services.Project
{
    public class SolutionGraph : ISolutionGraph
    {
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IRelationsGraphMapper _relationsGraphMapper;
        private readonly IGithubService _githubService;
        private readonly ICommentService _commentService;

        public SolutionGraph(IMapper mapper, IConfiguration configuration, IRelationsGraphMapper relationsGraphMapper, IGithubService githubService, ICommentService commentService)
        {
            _githubService = githubService;
            _mapper = mapper;
            _configuration = configuration;
            _relationsGraphMapper = relationsGraphMapper;
            _commentService = commentService;
        }

        public IEnumerable<Item> GetRelationsGraph(Solution solution)
        {
            List<Item> items = new List<Item>();
            foreach (var project in solution.Projects)
            {
                if (project.Language == _configuration["Framework:Language"])
                {
                    // This is needed to save File objects properly, otherwise old file objects
                    // are constantly being added to new projects and added up
                    List<Item> temporaryItemsList = new List<Item>();

                    // Get ALL methods inside of each file and save it into methodsRelations
                    foreach (var document in project.Documents)
                    {
                        // Exclude files which were auto-generated
                        if (!GeneratedCodeChecks.IsGeneratedFile(document.FilePath))
                        {
                            SyntaxNode root = document.GetSyntaxRootAsync().Result;
                            // Adding relations of document (file) and all associated Items
                            temporaryItemsList.AddRange(MapItemRelations(root, project, document));
                        }
                    }

                    var fileList = temporaryItemsList
                        .OfType<File>()
                        .ToList();

                    // Add Project
                    NETProcessorProject netProcessorProject = new NETProcessorProject(project.Id.Id.ToString(), project.Name, fileList);
                    // Add Parent to Files 
                    foreach(var file in fileList)
                    {
                        file.Parent = netProcessorProject;
                    }
                    items.AddRange(temporaryItemsList);
                    items.Add(netProcessorProject);
                }
            }

            // Add Child Classes of Interfaces, adding this in the outer loop, because interfaces relations to classes 
            // can be defined in different parts of the project or even in totally different projects
            var interfaceList = items
                            .OfType<Interface>()
                            .ToList();
            var classList = items
                            .OfType<Class>()
                            .ToList();

            foreach (var classInterface in interfaceList)
            {
                List<Class> cList = classList.Where(c => c.AttachedInterfaces.Contains(classInterface.Name)).ToList();
                classInterface.AddRangeChild(cList);
            }

            return items;
        }

        /*
        private Method RemoveThirdPartyMethods(Method method, List<Method> methodRelations)
        {
            // Set Ids for each child for being able to reference them later on edges (relations between nodes)
            // After all methods are mapped, we set the respective ids from the list
            // Remove methods that should not be in the graph like toString() by setting the child.id to 0
            // If it is not in our methods list, it iAs being removed and set to child.id 0
            // 
            // TODO: Remark: There might be methods that we need to keep, such as Async Methods that would
            // not show up if we remove them from the Graph, instead of removing the methods we should tag
            // them as third party, but still remove stuff like toString() by for example creating a custom filter
            // filtering out those methods by namespace or library
            foreach (var child in method.ChildList.Where(x => x.Id == "-1").ToList())
            {
                child.Id = methodRelations.Where(x => x.Name == child.Name).Select(x => x.Id).FirstOrDefault();
            }
            method.ChildList.RemoveAll(c => c.Id == null || c.Name == string.Empty);

            // Remove built or invalid methods, this is where all methods with id 0 are removed
            foreach (var child in method.ChildList)
            {
                .ChildList.RemoveAll(c => c.Id == null || c.Name == string.Empty);
            }

            return method;
        }
    */

        /// <summary>
        /// Mapping Method, Class, and Namespace relations
        /// </summary>
        /// <param name="root"></param>
        /// <param name="fileName"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        private List<Item> MapItemRelations(SyntaxNode root, Microsoft.CodeAnalysis.Project project, Document document)
        {
            string fileName = document.Name;
            Guid projectId = project.Id.Id;
            Guid fileId = document.Id.Id;
            string language = project.Language;

            DocumentWalker documentWalker = new DocumentWalker(root, projectId, fileName, fileId, language);
            documentWalker.Visit(root);

            // Adding all node types to generic item list class
            List<Item> itemList = new List<Item>();
            // Add Methods
            itemList.AddRange(documentWalker.methodsList);
            // Add Classes
            itemList.AddRange(documentWalker.classList);
            // Add Interfaces 
            itemList.AddRange(documentWalker.interfaceList);
            // Add File
            File file = new File(fileId.ToString(), fileName, projectId.ToString(), documentWalker.containingNamespace);
            itemList.Add(file);

            // Adding child and parent relationship for namespace
            documentWalker.containingNamespace.AddRangeChild(documentWalker.classList);
            documentWalker.containingNamespace.Parent = file;
            // Add Namespaces
            itemList.Add(documentWalker.containingNamespace);

            // Add Comments 
            // Get all comments and assigns comment to specific property id from the item list

            /*
            var comments = _commentService.GetCommentReferences(root, itemList.Where(x =>
                                        x.GetType() == typeof(Class) ||
                                        x.GetType() == typeof(Method) ||
                                        x.GetType() == typeof(Namespace))
                // TODO: Might need to add comments for interfaces here!
                                    .Select(x => new KeyValuePair<string, string>(x.Name, x.Id)));

            // Add all comments to respective nodes
            foreach(var item in itemList)
            {
                foreach(var comment in comments)
                {
                    if(comment.AttachedPropertyId.Equals(item.Id))
                    {
                        item.CommentList.Add(comment);
                    }
                }
            }
            */
            
            return itemList;
        }

        public async Task<ProjectRelationsGraph> ProcessRelationsGraph(IEnumerable<Item> relations, string solutionName, string repositoryToken)
        {
            List<Node> graphNodes = new List<Node>();

            // Sort nodes by hierarchy to allow for building data flow stream with GraphStreamGuid
            relations = relations.OrderBy(i => i.TypeHierarchy).ToList();

            foreach (var item in relations)
            {
                NodeRoot nodeBase = _mapper.Map<NodeRoot>(item);
                nodeBase.nodeTypeHierarchy = item.TypeHierarchy;
                nodeBase.nodeType = item.GetType().ToString().Split(".").Last();
                nodeBase.nodeData.nodeType = item.GetType().ToString().Split(".").Last();

                if (item is NETProcessorProject)
                {
                    NETProcessorProject netProcessorProject = (NETProcessorProject)item;
                    nodeBase.name = netProcessorProject.Name;
                    nodeBase.nodeData.name = netProcessorProject.Name;
                }
                else if (item is File)
                {
                    string graphStreamGuid = Guid.NewGuid().ToString();
                    File file = (File)item;
                    file.GraphStreamGuid.Add(graphStreamGuid);
                    nodeBase.name = file.Name;
                    nodeBase.nodeData.name = file.Name;
                    nodeBase.graphStreamGuid.Add(graphStreamGuid);
                }
                else if (item is Namespace)
                {
                    Namespace projectNamespace = (Namespace)item;
                    projectNamespace.GraphStreamGuid = projectNamespace.Parent.GraphStreamGuid;
                    nodeBase.name = projectNamespace.Name;
                    nodeBase.nodeData.name = projectNamespace.Name;
                    nodeBase.nodeData.comments = projectNamespace.CommentList;
                    nodeBase.nodeData.fileName = projectNamespace.FileName;
                    nodeBase.graphStreamGuid.Add(projectNamespace.Parent.GraphStreamGuid[0]);
                }
                else if (item is Interface)
                {
                    Interface i = (Interface)item;
                    i.GraphStreamGuid = i.Parent.GraphStreamGuid;
                    nodeBase.name = i.Name;
                    nodeBase.nodeData.name = i.Name;
                    nodeBase.nodeData.fileName = i.FileName;
                    nodeBase.nodeData.language = i.Language;

                    nodeBase.graphStreamGuid.Add(i.Parent.GraphStreamGuid[0]);
                }
                else if (item is Class)
                {
                    Class projectClass = (Class)item;
                    projectClass.GraphStreamGuid = projectClass.Parent.GraphStreamGuid;
                    nodeBase.name = projectClass.Name;
                    nodeBase.nodeData.name = projectClass.Name;
                    nodeBase.nodeData.fileName = projectClass.FileName;
                    nodeBase.nodeData.comments = projectClass.CommentList;
                    nodeBase.nodeData.language = projectClass.Language;

                    nodeBase.graphStreamGuid.Add(projectClass.Parent.GraphStreamGuid[0]);
                }
                else if (item is Method)
                {
                    Method method = (Method) item;
                    nodeBase.name = method.Name;
                    nodeBase.nodeData.name = method.Name;
                    nodeBase.nodeData.fileName = method.FileName;
                    nodeBase.nodeData.className = method.ClassName;
                    nodeBase.nodeData.language = method.Language;
                    nodeBase.nodeData.comments = method.CommentList;

                    nodeBase.graphStreamGuid.Add(method.Parent.GraphStreamGuid[0]);

                    // Pulling information for method from Repository (Github)
                    // TODO: This needs to be changed to the respective repository token from customer!
                    if (solutionName == "NETWebhookTest" || solutionName == "CoreWebhook")
                    {
                        GithubJSONResponse.Root githubJSONResponse = await GetRepositoryInformationForMethod(method.Name, method.FileName, "BennieBe", solutionName);
                        nodeBase.nodeData.repositoryCommitLinkOfMethod = githubJSONResponse.items[0]
                            .html_url.Replace("blob", "commits");
                        nodeBase.nodeData.repositoryLinkOfMethod = githubJSONResponse.items[0].html_url;
                    } else
                    {
                        nodeBase.nodeData.repositoryCommitLinkOfMethod = "GITHUB TOKEN OF CUSTOMER NEEDED TO GET THIS DATA!";
                        nodeBase.nodeData.repositoryLinkOfMethod = "GITHUB TOKEN OF CUSTOMER NEEDED TO GET THIS DATA!";
                    }

                    // Add all submethods as nodes to methods since they do not exist in the nodes tree yet
                    // AddChildMethodsAsNodes(graphNodes, method.ChildList, nodeBase.nodeData.nodeType);
                }
                else
                {
                    throw new Exception("The Item found for Nodes is not handled by any NodeType! Aborting program");
                }

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

        /*
        private void AddChildMethodsAsNodes(List<Node> graphNodes, List<Method> childList, string nodeType)
        {
            NodeRoot nodeBase = new NodeRoot();
            foreach (var child in childList)
            {
                nodeBase.Id = child.Id;
                nodeBase.name = child.Name;
                nodeBase.nodeType = nodeType;
                nodeBase.nodeData.name = child.Name;
                nodeBase.nodeData.fileName = child.FileName;
                nodeBase.nodeData.className = child.ClassName;
                nodeBase.nodeData.language = child.Language;
                nodeBase.nodeData.comments = child.CommentList;

                graphNodes.Add(new Node
                {
                    data = nodeBase
                });
            }
        }
        */

        private async Task<GithubJSONResponse.Root> GetRepositoryInformationForMethod(string methodName, string fileName, string repositoryOwner, string solutionName)
        {
            // Currently only one option Github, in future it could also come from AzureDevops
            GithubJSONResponse.Root githubJSONResponse = await _githubService.GetLinkToMethod(methodName, fileName, repositoryOwner, solutionName);
            return githubJSONResponse;
        }
    }
}