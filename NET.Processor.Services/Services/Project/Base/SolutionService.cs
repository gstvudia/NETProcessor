using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using NET.Processor.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using NET.Processor.Core.Models;
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using NET.Processor.Core.Models.RelationsGraph.Item;
using Lib2Git = LibGit2Sharp;
using NET.Processor.Core.Helpers;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using DynamicData;
using Microsoft.Extensions.Configuration;
using NET.Processor.Core.Models.RelationsGraph.Item.Base;
using System.IO;

namespace NET.Processor.Core.Services.Project
{
    public class SolutionService : ISolutionService
    {
        private readonly IDatabaseService _databaseService;
        private readonly ISolutionGraph _solutionGraph;
        private readonly IConfiguration _configuration;
        private readonly ICommentService _commentService;

        private Solution solution = null;
        private readonly string path = null;

        public SolutionService(IDatabaseService databaseService, ISolutionGraph solutionGraph, IConfiguration configuration, ICommentService commentService)
        {
            _solutionGraph = solutionGraph;
            _databaseService = databaseService;
            _commentService = commentService;
            _configuration = configuration;

            
            path = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())), "repos");
            // Create directory if not existing, otherwise do nothing
            System.IO.Directory.CreateDirectory(path);

            _databaseService.ConnectDatabase();

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
            string solutionPath = DirectoryHelper.FindFileInDirectory(path, repository.SolutionFilename);
            if (solutionPath != null)
            {
                try
                {
                    DirectoryHelper.ForceDeleteReadOnlyDirectory(solutionPath);
                } catch (Exception e)
                {
                    throw new Exception(
                    $"There was an error deleting the project under the following path: { solutionPath }, the error was: { e } ");
                }
            }
            string repositoryPath = Path.Combine(path, repository.SolutionName);
            try
            {
                var cloneOptions = new Lib2Git.CloneOptions
                {
                    //CredentialsProvider = (_url, _user, _cred) => new UsernamePasswordCredentials { Username = repository.User, Password = repository.Password }
                    CredentialsProvider = (_url, _user, _cred) => new Lib2Git.UsernamePasswordCredentials { Username = repository.Token, Password = string.Empty }
                };
                Lib2Git.Repository.Clone(repository.RepositoryURL, repositoryPath, cloneOptions);
            } catch (Exception e)
            {
                if (e is Lib2Git.NameConflictException)
                {
                    throw new Lib2Git.NameConflictException(
                    $"There was an error cloning the project with the following repository URL: { repository.RepositoryURL }, the repository already exists");
                }
                else if (e is Lib2Git.NotFoundException)
                {
                    throw new Lib2Git.NotFoundException(
                    $"There was an error cloning the project with the following repository URL: { repository.RepositoryURL }, the repository specified could not be found");
                }
                else
                {
                    throw new Exception(
                        $"There was an error cloning the project with the following repository URL: { repository.RepositoryURL }, the error was: { e } ");
                }
            }
        }

        public async Task<Solution> LoadSolution(string solutionName, string solutionFilename)
        {
            string solutionPath = DirectoryHelper.FindFileInDirectory(path, solutionFilename);
            // If solution to process could not be found, throw exception
            if (solutionPath == null)
            {
                throw new Lib2Git.NotFoundException(
                    $"The specified solution under the solution path: { solutionPath } could not be found.");
            }

            // Solution base path "\\source\\repos\\Solutions\\ + {{ SolutionName }} + \\ {{ SolutionFilename }}.sln
            solutionPath = Path.Combine(solutionPath, solutionFilename + ".sln");

            using var msWorkspace = CreateMSBuildWorkspace();
            try
            {
                solution = await msWorkspace.OpenSolutionAsync(solutionPath);

                // TODO: We can log diagnosis later
                ImmutableList<WorkspaceDiagnostic> diagnostics = msWorkspace.Diagnostics;

            }
            catch (Exception e)
            {
                throw new Exception(
                    $"There was an error opening the project with the following path: { solutionPath }, the error was: { e } ");
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
                throw new Exception(
                    $"There was an error creating the MSBuildWorkspace, the error was: { e } ");
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

                        /*
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
                        */
                        //var methodList = MapMethods(root);
                        //list.AddRange(methodList);

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
                        /*var commentReferences = _commentService.GetCommentReferences(root, list.Where(x =>
                                                    x.GetType() == typeof(Class) ||
                                                    x.GetType() == typeof(Method) ||
                                                    x.GetType() == typeof(Namespace))
                                                .Select(x => new KeyValuePair<string, int>(x.Name, x.Id)));
                        var comments = commentReferences.Select(x => new Comment(x.LineNumber, x.Name, x.AttachedPropertyId, x.AttachedPropertyName, x.MethodOrPropertyIfAny, x.TypeIfAny, x.NamespaceIfAny))
                                .ToList();
                        list.AddRange(comments);
                        */
                    }
                }
                // Get all documents from one project and add them to list
                //var documents = project.Documents.Select(x => new Models.RelationsGraph.Item.File(x.Id.Id, x.Name.Split('.')[0].ToString()))
                //                .ToList();
                //list.AddRange(documents);
            }
            // TODO: Reason about whether it makes sense to also add another entry per project in mongodb
            //var projects = solution.Projects.Select(x => new Models.RelationsGraph.Item.Project(x.Id.Id, x.Name.ToString()))
            // .ToList();
            //list.AddRange(projects);

            return list;
        }

        public IEnumerable<Item> GetRelationsGraph(Solution solution)
        {
            return _solutionGraph.GetRelationsGraph(solution);
        }

        public void ProcessRelationsGraph(IEnumerable<Item> relations, string solutionName, string repositoryToken)
        {
            ProjectRelationsGraph relationGraph = _solutionGraph.ProcessRelationsGraph(relations, solutionName, repositoryToken).Result;
            // Store collection in Database
            _databaseService.StoreGraphNodesAndEdges(relationGraph);
            // Remark: No need to close db again, handled by database engine (MongoDB)
        }
    }
}
