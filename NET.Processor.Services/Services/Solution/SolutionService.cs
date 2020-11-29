using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using NET.Processor.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using NET.Processor.Core.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using DynamicData;
using Microsoft.Extensions.Configuration;
using NET.Processor.Core.Models.RelationsGraph.Item;
using LibGit2Sharp;
using System.Net.Http;
using System.Net;
using System.Web.Http;
using NET.Processor.Core.Helpers;

namespace NET.Processor.Core.Services
{
    public class SolutionService : ISolutionService
    {
        private readonly ICommentService _commentService;
        private readonly IConfiguration _configuration;
        private Solution solution = null;
        private static readonly string homeDrive = Environment.GetEnvironmentVariable("HOMEDRIVE");
        private static readonly string homePath = Environment.GetEnvironmentVariable("HOMEPATH");
        private readonly string path = @"" + homeDrive + homePath + "\\source\\repos\\Solutions\\";
           
        public SolutionService(ICommentService commentService, IConfiguration configuration)
        {
            _commentService = commentService;
            _configuration = configuration;

            if (!MSBuildLocator.IsRegistered)
            {
                MSBuildLocator.RegisterDefaults();
            }

            // Automapper configuration
            //_automapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Item, ItemDTO>()
            //    .ForMember(dest => dest.ParentId, act => act.MapFrom(src => src.Parent.Id))
            //);
        }

        /// <summary>
        /// Get repository to load solution from
        /// </summary>
        /// <param name="repositoryName"></param>
        /// <returns></returns>
        public void SaveSolutionFromRepository(CodeRepository repository)
        {
            // If path exists, remove old solution and add new one
            string solutionPath = DirectoryHelper.FindFileInDirectory(path, repository.ProjectFilename);
            if(solutionPath != null)
            {
                try
                {
                    DirectoryHelper.ForceDeleteReadOnlyDirectory(solutionPath);
                } catch(Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
            string repositoryPath = path + repository.ProjectName;
            try
            {
                var cloneOptions = new CloneOptions
                {
                    // CredentialsProvider = (_url, _user, _cred) => new UsernamePasswordCredentials { Username = repository.User, Password = repository.Password }
                    CredentialsProvider = (_url, _user, _cred) => new UsernamePasswordCredentials { Username = repository.Token, Password = string.Empty }
            };
                Repository.Clone(repository.RepositoryURL, repositoryPath, cloneOptions);
            } catch(Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<Solution> LoadSolution(string solutionName, string solutionFilename)
        {
            string solutionPath = DirectoryHelper.FindFileInDirectory(path, solutionFilename);
            // If solution to process could not be found, throw exception
            if (solutionPath == null)
            {
                // TODO: Needs to properly return HttpStatusCode to frontend and show the message as shown below!
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent("Solution could not be found, please check if the path you use is correct"),
                    ReasonPhrase = "Path not found"
                });
            }

            // Solution base path "\\source\\repos\\Solutions\\ + {{ ProjectName }} + \\ {{ SolutionFilename }}.sln
            solutionPath = solutionPath + "\\" + solutionFilename + ".sln";

            using var msWorkspace = CreateMSBuildWorkspace();
            try
            {
                solution = await msWorkspace.OpenSolutionAsync(solutionPath);

                //TODO: We can log diagnosis later
                ImmutableList<WorkspaceDiagnostic> diagnostics = msWorkspace.Diagnostics;

            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }

            return solution;
        }

        private MSBuildWorkspace CreateMSBuildWorkspace()
        {
            MSBuildWorkspace msWorkspace = null;

            try
            {
                msWorkspace = MSBuildWorkspace.Create();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return msWorkspace;
        }

        public IEnumerable<Item> GetSolutionItems(Solution solution)
        {
            SyntaxNode root = null;
            var list = new List<Item>();
            List<RegionDirectiveTriviaSyntax> regionDirectives = null;
            List<EndRegionDirectiveTriviaSyntax> endRegionDirectives = null;
            EndRegionDirectiveTriviaSyntax endNode = null;
            List<Method> methods = new List<Method>();

            foreach (var project in solution.Projects)
            {
                if (project.Language == _configuration["Framework:Language"])
                {
                    foreach (var document in project.Documents)
                    {
                        root = document.GetSyntaxRootAsync().Result;

                        regionDirectives = root.DescendantNodes(null, true).OfType<RegionDirectiveTriviaSyntax>().Reverse().ToList();
                        endRegionDirectives = root.DescendantNodes(null, true).OfType<EndRegionDirectiveTriviaSyntax>().ToList();

                        foreach (var startNode in regionDirectives)
                        {
                            endNode = endRegionDirectives.First(r => r.SpanStart > startNode.SpanStart);
                            endRegionDirectives.Remove(endNode);

                            list.Add(new Region(startNode.ToString()));
                        }

                        var namespaces = root.DescendantNodes()
                                     .OfType<NamespaceDeclarationSyntax>()
                                     .Select(x => new Namespace(root.DescendantNodes().IndexOf(x), x.Name.ToString()))
                                     .ToList();

                        if (namespaces.Count > 1)
                            list.AddRange(namespaces);

                        var classes = root.DescendantNodes()
                                          .OfType<ClassDeclarationSyntax>()
                                          .Select(x => new Class(root.DescendantNodes().IndexOf(x), x.Identifier.ValueText))
                                          .ToList();

                        list.AddRange(classes);

                        MapMethods(root);

                        /*
                        var interfaces = root.DescendantNodes()
                                             .OfType<InterfaceDeclarationSyntax>()
                                             .Select(x => new Item(root.DescendantNodes().IndexOf(x), x.Identifier.ValueText, ItemType.Interface, x.Span))
                                             .ToList();

                        list.AddRange(interfaces);

                        var enums = root.DescendantNodes()
                                             .OfType<EnumDeclarationSyntax>()
                                             .Select(x => new Item(root.DescendantNodes().IndexOf(x), x.Identifier.ValueText, ItemType.Enum, x.Span))
                                             .ToList();

                        list.AddRange(enums);

                        var enumMembers = root.DescendantNodes()
                                             .OfType<EnumMemberDeclarationSyntax>()
                                             .Select(x => new Item(root.DescendantNodes().IndexOf(x), x.Identifier.ValueText, ItemType.EnumMember, x.Span))
                                             .ToList();

                        list.AddRange(enumMembers);

                        var structs = root.DescendantNodes()
                                             .OfType<StructDeclarationSyntax>()
                                             .Select(x => new Item(root.DescendantNodes().IndexOf(x), x.Identifier.ValueText, ItemType.Struct, x.Span))
                                             .ToList();

                        list.AddRange(structs);

                        var constructors = root.DescendantNodes()
                                               .OfType<ConstructorDeclarationSyntax>()
                                               .Select(x => new Item(root.DescendantNodes().IndexOf(x),x.Identifier.ValueText, ItemType.Constructor, x.Span))
                                               .ToList();

                        list.AddRange(constructors);
                        

                        var fields = root.DescendantNodes()
                                         .OfType<FieldDeclarationSyntax>()
                                         .SelectMany(x => x.Declaration.Variables,
                                                     (f, v) => f.Modifiers.Any(SyntaxKind.ConstKeyword) ? new Item(root.DescendantNodes().IndexOf(v),v.Identifier.ValueText, ItemType.Const, v.Span)
                                                                                                        : new Item(root.DescendantNodes().IndexOf(v), v.Identifier.ValueText, ItemType.Field, v.Span))
                                         .ToList();

                        list.AddRange(fields);

                        var properties = root.DescendantNodes()
                                             .OfType<PropertyDeclarationSyntax>()
                                             .Select(x => new Item(root.DescendantNodes().IndexOf(x), x.Identifier.ValueText, ItemType.Property, x.Span))
                                             .ToList();

                        list.AddRange(properties);

                        var eventFields = root.DescendantNodes()
                                              .OfType<EventFieldDeclarationSyntax>()
                                              .SelectMany(x => x.Declaration.Variables, (f, v) => new Item(root.DescendantNodes().IndexOf(v),v.Identifier.ValueText, ItemType.Event, v.Span))
                                              .ToList();

                        list.AddRange(eventFields);

                        var eventProperties = root.DescendantNodes()
                                                  .OfType<EventDeclarationSyntax>()
                                                  .Select(x => new Item(root.DescendantNodes().IndexOf(x), x.Identifier.ValueText, ItemType.Event, x.Span))
                                                  .ToList();

                        list.AddRange(eventProperties);

                        var delegates = root.DescendantNodes()
                                             .OfType<DelegateDeclarationSyntax>()
                                             .Select(x => new Item(root.DescendantNodes().IndexOf(x), x.Identifier.ValueText, ItemType.Delegate, x.Span))
                                             .ToList();

                        list.AddRange(delegates);
                        */

                        //End of document/class

                        // Get all comments and assigns comment to specific property id from the item list
                        var commentReferences = _commentService.GetCommentReferences(root, list.Where(x => x.GetType() == typeof(Class) ||
                                                x.GetType() == typeof(Method) ||
                                                x.GetType() == typeof(Namespace))
                                                .Select(x => new KeyValuePair<string, int>(x.Name, x.Id)));
                        var comments = commentReferences.Select(x => new Comment(x.LineNumber, x.Name, x.AttachedPropertyId, x.AttachedPropertyName, x.MethodOrPropertyIfAny, x.TypeIfAny, x.NamespaceIfAny))
                                .ToList();
                        list.AddRange(comments);
                    }
                }
                // Get all documents from one project and add them to list
                var documents = project.Documents.Select(x => new NodeDocument(x.Id.Id, x.Name.Split('.')[0].ToString()))
                                .ToList();
                list.AddRange(documents);
            }
            // TODO: Reason about whether it makes sense to also add another entry per project in mongodb
            var projects = solution.Projects.Select(x => new NodeProject(x.Id.Id, x.Name.ToString()))
             .ToList();
            list.AddRange(projects);

            return list;
        }

        public IEnumerable<Method> GetRelationsGraph(Solution solution)
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
                        methodsRelations.AddRange(MapMethods(root));
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

        private List<Method> MapMethods(SyntaxNode root)
        {
            List<Method> methodsList = new List<Method>();
            var methodNodes = root.DescendantNodes().OfType<MethodDeclarationSyntax>();
            var methods = methodNodes
                                     .Select(x => new Method(root.DescendantNodes().IndexOf(x), x.Identifier.ValueText, x.Body))
                                     .ToList();

            foreach (var method in methods)
            {                
                if(!methodsList.Any(x=>x.Name == method.Name))
                {
                    method.ChildList.AddRange(GetChilds(method));
                    methodsList.Add(method);
                }                
            }

            return methodsList;
        }
        private int GetMethodId(string methodName, List<Method> AllMethods)
        {
            var methodExists = AllMethods.Where(m => m.Name == methodName).FirstOrDefault();
            if (methodExists == null)
            {
                return AllMethods.Max(m => m.Id) + 1;
            }
            else
            {
                return methodExists.Id;
            }
        }

        private List<Method> GetChilds(Method method)
        {
            List<Method> childList = new List<Method>();

            if(method.Body != null)
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
                                -1, child[1].Substring(0, child[1].LastIndexOf("(") + 1).Replace("(", string.Empty)
                            ));
                    }
                    else
                    {
                        childList
                            .Add(new Method(
                                -1, child[0].Substring(0, child[0].LastIndexOf("(") + 1).Replace("(", string.Empty)
                            ));
                    }
                }
            }            

            return childList;
        }
    }
}
