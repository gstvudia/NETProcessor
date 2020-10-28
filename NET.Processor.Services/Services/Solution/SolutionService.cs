using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using NET.Processor.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NET.Processor.Core.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp;
using NET.Processor.Core.Helpers;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using Microsoft.Build.Locator;
using DynamicData;
using Microsoft.Extensions.Configuration;
using AutoMapper;
using NET.Processor.Core.Models.RelationsGraph.Item;
using LibGit2Sharp;

namespace NET.Processor.Core.Services
{
    public class SolutionService : ISolutionService
    {
        private readonly ICommentService _commentService;
        private readonly IDatabaseService _databaseService;
        private readonly IConfiguration _configuration;
        private Solution solution = null;
        private string currentSolutionPath = "";
        private string homeDrive = Environment.GetEnvironmentVariable("HOMEDRIVE");
        private string homePath = Environment.GetEnvironmentVariable("HOMEPATH");

        public SolutionService(ICommentService commentService, IDatabaseService databaseService, IConfiguration configuration)
        {
            _commentService = commentService;
            _databaseService = databaseService;
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
        public Task<Solution> LoadSolutionFromRepository(WebHook webhook)
        {
            string path = @"" + homeDrive + homePath + "\\source\\repos\\Solutions\\" + webhook.SolutionName;
            try
            {
                var cloneOptions = new CloneOptions();
                cloneOptions.CredentialsProvider = (_url, _user, _cred) => new UsernamePasswordCredentials { Username = webhook.User, Password = webhook.Password };
                Repository.Clone(webhook.RepositoryURL, path, cloneOptions);
            } catch(Exception e)
            {
                throw new Exception(e.Message);
            }
            return null;
        }

        public async Task<Solution> LoadSolution(string newSolutionPath)
        {
            var path = string.Empty;
            // Skip loading of solution, if solution has already been loaded
            if (solution != null && newSolutionPath == currentSolutionPath)
            {
                return solution;
            }
            currentSolutionPath = newSolutionPath;

            switch (newSolutionPath)
            {
                case "CleanArchitecture":
                    path = @"" + homeDrive + homePath + "\\source\\repos\\Solutions\\CleanArchitecture-master\\CleanArchitecture.sln";
                    break;
                case "TestProject":
                    path = @"" + homeDrive + homePath + "\\source\\repos\\NETWebhookTest\\TestProject.sln";
                    break;
            }

            using (var msWorkspace = CreateMSBuildWorkspace())
            {
                try
                {                    
                    solution = await msWorkspace.OpenSolutionAsync(path);

                    //TODO: We can log diagnosis later
                    ImmutableList<WorkspaceDiagnostic> diagnostics = msWorkspace.Diagnostics;

                }
                catch (Exception ex) {
                    
                }
            
                return solution;
            }
        }

        private MSBuildWorkspace CreateMSBuildWorkspace()
        {
            MSBuildWorkspace msWorkspace = null;

            try
            {
                msWorkspace = MSBuildWorkspace.Create();
            } catch(Exception e) {
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

                            list.Add(new Region(startNode.ToString(), new TextSpan(startNode.Span.Start, endNode.Span.End - startNode.Span.Start)));
                        }
                        
                        var namespaces = root.DescendantNodes()
                                     .OfType<NamespaceDeclarationSyntax>()
                                     .Select(x => new Namespace(root.DescendantNodes().IndexOf(x), x.Name.ToString(), x.Span))
                                     .ToList();

                        if (namespaces.Count > 1)
                            list.AddRange(namespaces);

                        var classes = root.DescendantNodes()
                                          .OfType<ClassDeclarationSyntax>()
                                          .Select(x => new Class(root.DescendantNodes().IndexOf(x), x.Identifier.ValueText, x.Span))
                                          .ToList();

                        list.AddRange(classes);

                        var methods = root.DescendantNodes()
                                          .OfType<MethodDeclarationSyntax>()
                                          .Select(x => new Method(root.DescendantNodes().IndexOf(x), x.Identifier.ValueText, x.Span))
                                          .Where(x => !list.Any(l => l.Name == x.Name))
                                          .ToList();
                        list.AddRange(methods);

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
                        var commentReferences = _commentService.GetCommentReferences(root, list
                                                .Where(x => x.GetType() == typeof(Class) || 
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
    }
}
