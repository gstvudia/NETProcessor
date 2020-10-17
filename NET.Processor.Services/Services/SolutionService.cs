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

namespace NET.Processor.Core.Services
{
    public class SolutionService : ISolutionService
    {
        private readonly ICommentService _commentService;
        private readonly IConfiguration _configuration;

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

        public async Task<Solution> LoadSolution(string solutionPath)
        {
            var homeDrive = Environment.GetEnvironmentVariable("HOMEDRIVE");
            var homePath = Environment.GetEnvironmentVariable("HOMEPATH");
            string path = string.Empty;

            switch (solutionPath)
            {
                case "CleanArchitecture":
                    path = @"" + homeDrive + homePath + "\\source\\repos\\Solutions\\CleanArchitecture-master\\CleanArchitecture.sln";
                    break;
                case "TestProject":
                    path = @"" + homeDrive + homePath + "\\source\\repos\\NETWebhookTest\\TestProject.sln";
                    break;
            }

            Solution solution = null;
            using (var msWorkspace = MSBuildWorkspace.Create())
            {
                try
                {                    
                    solution = await msWorkspace.OpenSolutionAsync(path);

                    //TODO: We can log diagnosis later
                    ImmutableList<WorkspaceDiagnostic> diagnostics = msWorkspace.Diagnostics;

                }
                catch (Exception){}
            
                return solution;
            }

        }

        public IEnumerable<Item> GetSolutionItems(Solution solution, Filter filter)
        {
            SyntaxNode root = null;
            var list = new List<Item>();
            List<RegionDirectiveTriviaSyntax> regionDirectives = null;
            List<EndRegionDirectiveTriviaSyntax> endRegionDirectives = null;
            EndRegionDirectiveTriviaSyntax endNode = null;

            // Selecting Projects based on Project Filter
            IEnumerable<Project> selectedProjects = RelationsGraphFilter.FilterSolutions(solution, filter);
            foreach (var project in selectedProjects)
            {
                if (project.Language == _configuration["Framework:Language"])
                {
                    // Selecting Documents based on Document Filter
                    IEnumerable<Document> selectedDocuments = RelationsGraphFilter.FilterDocuments(project, filter);
                    foreach (var document in selectedDocuments)
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

                        // TODO: There must be a proper ID assigned to each project
                        var projects = selectedProjects.Select(x => new NodeProject(x.Id.Id, x.Name.ToString()))
                                     .ToList();
                        list.AddRange(projects);

                        var documents = selectedDocuments.Select(x => new NodeDocument(x.Id.Id, x.Name.Split('.')[0].ToString()))
                                     .ToList();
                        list.AddRange(documents);

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

                        // Select found methods based on Methods filter
                        IEnumerable<Item> filteredMethods = RelationsGraphFilter.FilterMethods(methods, filter);
                        list.AddRange(filteredMethods);

                        var commentReferences = _commentService.GetCommentReferences(root);
                        var comments = commentReferences.Select(x => new Comment(x.LineNumber, x.Name, x.MethodOrPropertyIfAny, x.TypeIfAny, x.NamespaceIfAny))
                                .ToList();
                        list.AddRange(comments);

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
                    }
                }
            }

            return list;
        }










        // TODO: Decide if this is still needed, otherwise delete
        public async Task<Stream> GetContentsFromRepo(string contentsUrl, HttpClient httpClient)
        {
            var repository = await httpClient.GetStreamAsync(contentsUrl);
            return repository;
        }

        // TODO: Implement loading from Repo instead of locally
        public List<string> GetSolutionFromRepo(WebHook webHook)
        {
            List<string> DirectoryFiles = new List<string>();
            try
            {
                //CHECK IF FOLDER EXISTS ADN REMOVE FIRST
                //Repository.Clone("https://github.com/ardalis/CleanArchitecture.git", Directory.GetParent(Directory.GetCurrentDirectory()).FullName + "/Clones");

                //get file names or something like that and add on the list
                //DirectoryFiles.Add();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }


            return DirectoryFiles;
        }
    }
}
