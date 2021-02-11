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

            // Namespace here is needed to save namespaces outside of projects to ensure that several namespaces
            // can bundle all child classes and its ids, it ensures also that namespace can connect to all file 
            // nodes on frontend!
            List<Namespace> namespacesList = new List<Namespace>();

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
                            temporaryItemsList.AddRange(MapItemRelations(root, project, document, namespacesList));
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

            // Add all namespaces from namespacesList which were previously gathered together with all Childs and Parents
            items.AddRange(namespacesList);

            // Add Child Classes of Interfaces, adding this in the outer loop, because interfaces relations to classes 
            // can be defined in different parts of the project or even in totally different projects
            var interfaceList = items
                            .OfType<Interface>()
                            .ToList();
            var classList = items
                            .OfType<Class>()
                            .ToList();

            var methodsList = items
                            .OfType<Method>()
                            .ToList();

            foreach (var classInterface in interfaceList)
            {
                List<Class> cList = classList.Where(c => c.AttachedInterfaces.Contains(classInterface.Name)).ToList();
                classInterface.AddRangeChild(cList);
            }

            // Building the graph stream guid system to allow for quick search by name on frontend
            // By assigning each node of the stream the same unique graph stream guid
            items = BuildGraphStreamGuidSystem(items);

            foreach(var method in methodsList)
            {
                foreach (var child in method.ChildList)
                {
                    child.Id = methodsList.FirstOrDefault(m => m.Name == child.Name)?.Id;
                }

                //Remove unmapped methods from childs
                method.ChildList.RemoveAll(c => c.Id == null);
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
        private List<Item> MapItemRelations(SyntaxNode root, Microsoft.CodeAnalysis.Project project, Document document, List<Namespace> namespacesList)
        {
            string fileName = document.Name;
            Guid projectId = project.Id.Id;
            Guid fileId = document.Id.Id;
            string language = project.Language;
            
            DocumentWalker documentWalker = new DocumentWalker(root, projectId, fileName, fileId, language, namespacesList);
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

            // Add childs and parent to Namespace and then add Namespace to itemList
            AddNamespaceToItems(namespacesList, documentWalker, file);

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

        private void AddNamespaceToItems(List<Namespace> namespacesList, DocumentWalker documentWalker, File file)
        {
            Namespace n = namespacesList.Where(i => i.Name == documentWalker.containingNamespace.Name).FirstOrDefault();
            if(n != null)
            {
                foreach (var c in documentWalker.classList)
                {
                    // Adding child and parent relationship for namespace    
                    if (n.ChildList.SingleOrDefault(n => n.Name.Equals(c.Name)) == null)
                    {
                        n.AddChild(c);
                    }
                }
                    n.ParentList.Add(file);
                } else
            {
                // Add child classes to namespace
                documentWalker.containingNamespace.AddRangeChild(documentWalker.classList);
                // Add parent of file
                documentWalker.containingNamespace.ParentList.Add(file);
                // Add namespace to list
                namespacesList.Add(documentWalker.containingNamespace);
            }
        }

        private List<Item> BuildGraphStreamGuidSystem(List<Item> items)
        {
            // Assign all classes unique stream guid
            foreach(var item in items.OfType<Class>())
            {
                string graphStreamGuid = Guid.NewGuid().ToString();
                item.GraphStreamGuid.Add(graphStreamGuid);
            }
            // Assign methods to unique stream guid of parent classes
            foreach (var item in items.OfType<Method>())
            {
                item.GraphStreamGuid = item.Parent.GraphStreamGuid;
            }
            // Adding unique stream guid to class childs of interface
            foreach (var item in items.OfType<Interface>())
            {
                item.GraphStreamGuid.AddRange(item.ChildList.SelectMany(cl => cl.GraphStreamGuid));
            }
            // Adding unique stream guid to namespace
            foreach(var item in items.OfType<Namespace>())
            {
                if (item.GraphStreamGuid.Count == 0)
                {
                    item.GraphStreamGuid.AddRange(item.ChildList.SelectMany(cl => cl.GraphStreamGuid));
                }
            }
            // Adding unique stream guid to file
            foreach (var item in items.OfType<File>())
            {
                item.GraphStreamGuid = item.Child.GraphStreamGuid;
            }
            // Adding unique stream guid to file
            foreach (var item in items.OfType<NETProcessorProject>())
            {
                item.GraphStreamGuid.AddRange(item.ChildList.SelectMany(fl => fl.GraphStreamGuid));
            }

            return items;
        }

        public async Task<ProjectRelationsGraph> ProcessRelationsGraph(IEnumerable<Item> relations, string solutionName, string repositoryToken)
        {
            List<Node> graphNodes = new List<Node>();

            foreach (var item in relations)
            {
                NodeRoot nodeBase = new NodeRoot();
                nodeBase.Id = MongoDB.Bson.ObjectId.GenerateNewId();
                nodeBase.id = item.Id;
                nodeBase.nodeTypeHierarchy = item.TypeHierarchy;
                nodeBase.nodeType = item.GetType().ToString().Split(".").Last();
                nodeBase.nodeData.nodeType = item.GetType().ToString().Split(".").Last();

                if (item is NETProcessorProject)
                {
                    NETProcessorProject netProcessorProject = (NETProcessorProject)item;
                    nodeBase.name = netProcessorProject.Name;
                    nodeBase.graphStreamGuid = netProcessorProject.GraphStreamGuid;
                    nodeBase.nodeData.name = netProcessorProject.Name;
                }
                else if (item is File)
                {
                    File file = (File)item;
                    nodeBase.name = file.Name;
                    nodeBase.graphStreamGuid = file.GraphStreamGuid;
                    nodeBase.nodeData.name = file.Name;
                }
                else if (item is Namespace)
                {
                    Namespace projectNamespace = (Namespace)item;
                    nodeBase.name = projectNamespace.Name;
                    nodeBase.graphStreamGuid = projectNamespace.GraphStreamGuid;
                    nodeBase.nodeData.name = projectNamespace.Name;
                    nodeBase.nodeData.comments = projectNamespace.CommentList;
                    nodeBase.nodeData.fileName = projectNamespace.FileName;
                }
                else if (item is Interface)
                {
                    Interface i = (Interface)item;
                    nodeBase.name = i.Name;
                    nodeBase.graphStreamGuid = i.GraphStreamGuid;
                    nodeBase.nodeData.name = i.Name;
                    nodeBase.nodeData.fileName = i.FileName;
                    nodeBase.nodeData.language = i.Language;
                }
                else if (item is Class)
                {
                    Class projectClass = (Class)item;
                    nodeBase.name = projectClass.Name;
                    nodeBase.graphStreamGuid = projectClass.GraphStreamGuid;
                    nodeBase.nodeData.name = projectClass.Name;
                    nodeBase.nodeData.fileName = projectClass.FileName;
                    nodeBase.nodeData.comments = projectClass.CommentList;
                    nodeBase.nodeData.language = projectClass.Language;
                }
                else if (item is Method)
                {
                    Method method = (Method) item;
                    nodeBase.name = method.Name;
                    nodeBase.graphStreamGuid = method.GraphStreamGuid;
                    nodeBase.nodeData.name = method.Name;
                    nodeBase.nodeData.parameterList = method.NodeParameters();
                    nodeBase.nodeData.returnType = method.ReturnType;
                    nodeBase.nodeData.fileName = method.FileName;
                    nodeBase.nodeData.className = method.ClassName;
                    nodeBase.nodeData.language = method.Language;
                    nodeBase.nodeData.comments = method.CommentList;

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