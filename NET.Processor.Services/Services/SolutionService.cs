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
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using LibGit2Sharp;

namespace NET.Processor.Core.Services
{
    public class SolutionService : ISolutionService
    {
        public Solution LoadSolution(string solutionPath)
        {
            Solution solution = null;

            using (var msWorkspace = MSBuildWorkspace.Create())
            {
                try
                {
                    var co = new CloneOptions();
                    //co.CredentialsProvider = (_url, _user, _cred) => new UsernamePasswordCredentials { Username = "Username", Password = "Password" };
                    //Repository.Clone("https://github.com/ardalis/CleanArchitecture.git", Directory.GetParent(Directory.GetCurrentDirectory()).FullName + "/Clones");
                    solutionPath = Directory.GetParent(Directory.GetCurrentDirectory()).FullName + "\\Clones\\CleanArchitecture.sln";
                    solution = msWorkspace.OpenSolutionAsync(solutionPath).Result;

                    //We can log diagnosis later
                    ImmutableList<WorkspaceDiagnostic> diagnostics = msWorkspace.Diagnostics;
                    foreach (var diagnostic in diagnostics)
                    {
                        //Console.WriteLine(diagnostic.Message);
                    }
                }
                catch (Exception ex)
                {
                    //We can log diagnosis later
                    ImmutableList<WorkspaceDiagnostic> diagnostics = msWorkspace.Diagnostics;
                    foreach (var diagnostic in diagnostics)
                    {
                        //Console.WriteLine(diagnostic.Message);
                    }
                }
            
                return solution;
            }

        }
        public async Task<Stream> GetContentsFromRepo(string contentsUrl, HttpClient httpClient)
        { 
            var a = await httpClient.GetStreamAsync(contentsUrl);
            return a;
        }

        public List<string> GetSolutionFromRepo(WebHook webHook)
        {
            List<string> DirectoryFiles = new List<string>();
            try
            {
                //CHECK IF FOLDER EXISTS ADN REMOVE FIRST
                Repository.Clone("https://github.com/ardalis/CleanArchitecture.git", Directory.GetParent(Directory.GetCurrentDirectory()).FullName + "/Clones");

                //get file names or something like that and add on the list
                //DirectoryFiles.Add();
            }
            catch (Exception ex)
            {
                Console.WriteLine();
            }
            

            return DirectoryFiles;
        }



        //public IEnumerable<FileInfo> LoadFilePaths(string solutionFilePath)
        //{
        //    List<FileInfo> cSharpCompileFileList = new List<FileInfo>();
        //
        //    foreach (var csharpCompileFile in GetProjectFilesForSolution(new FileInfo(solutionFilePath)).SelectMany(projectFile => GetCSharpCompileItemFilesForProject(projectFile)))
        //    {
        //        cSharpCompileFileList.Add(csharpCompileFile);
        //    }
        //
        //    return cSharpCompileFileList;
        //}

        private static IEnumerable<FileInfo> GetProjectFilesForSolution(FileInfo solutionFile)
        {
            if (solutionFile == null)
                throw new ArgumentNullException("solutionFile");

            var projectFileMatcher = new Regex(
                @"Project\(""\{\w{8}-\w{4}-\w{4}-\w{4}-\w{12}\}""\) = ""(.*?)"", ""(?<projectFile>(.*?\.csproj))"", ""\{\w{8}-\w{4}-\w{4}-\w{4}-\w{12}\}"""
            );
            foreach (Match match in projectFileMatcher.Matches(solutionFile.OpenText().ReadToEnd()))
                yield return new FileInfo(Path.Combine(solutionFile.Directory.FullName, match.Groups["projectFile"].Value));
        }

        //private static IEnumerable<FileInfo> GetCSharpCompileItemFilesForProject(FileInfo projectFile)
        //{
        //    if (projectFile == null)
        //        throw new ArgumentNullException("projectFile");
        //
        //    return (new ProjectCollection()).LoadProject(projectFile.FullName).AllEvaluatedItems
        //        .Where(item => item.ItemType == "Compile")
        //        .Select(item => item.EvaluatedInclude)
        //        .Where(include => include.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
        //        .Select(include => new FileInfo(Path.Combine(projectFile.Directory.FullName, include)));
        //}

        //IObservable<List<Item>>
        public List<Item> GetSolutionItems(Solution solution)
        {
            SyntaxNode root = null;
            var list = new List<Item>();
            List<RegionDirectiveTriviaSyntax> regionDirectives = null;
            List<EndRegionDirectiveTriviaSyntax> endRegionDirectives = null;
            EndRegionDirectiveTriviaSyntax endNode = null;

            foreach (var project in solution.Projects)
            {
                if (project.Language == "C#")
                {
                    foreach (var documentClass in project.Documents)
                    {
                        root = documentClass.GetSyntaxRootAsync().Result;

                        regionDirectives = root.DescendantNodes(null, true).OfType<RegionDirectiveTriviaSyntax>().Reverse().ToList();
                        endRegionDirectives = root.DescendantNodes(null, true).OfType<EndRegionDirectiveTriviaSyntax>().ToList();

                        foreach (var startNode in regionDirectives)
                        {
                            endNode = endRegionDirectives.First(r => r.SpanStart > startNode.SpanStart);
                            endRegionDirectives.Remove(endNode);

                            list.Add(new Item(startNode.ToString(), ItemType.Region, new TextSpan(startNode.Span.Start, endNode.Span.End - startNode.Span.Start)));
                        }


                        var namespaces = root.DescendantNodes()
                                     .OfType<NamespaceDeclarationSyntax>()
                                     .Select(x => new Item(x.Name.ToString(), ItemType.Namespace, x.Span))
                                     .ToList();

                        if (namespaces.Count > 1)
                            list.AddRange(namespaces);

                        var classes = root.DescendantNodes()
                                          .OfType<ClassDeclarationSyntax>()
                                          .Select(x => new Item(x.Identifier.ValueText, ItemType.Class, x.Span))
                                          .ToList();

                        list.AddRange(classes);

                        var interfaces = root.DescendantNodes()
                                             .OfType<InterfaceDeclarationSyntax>()
                                             .Select(x => new Item(x.Identifier.ValueText, ItemType.Interface, x.Span))
                                             .ToList();

                        list.AddRange(interfaces);

                        var enums = root.DescendantNodes()
                                             .OfType<EnumDeclarationSyntax>()
                                             .Select(x => new Item(x.Identifier.ValueText, ItemType.Enum, x.Span))
                                             .ToList();

                        list.AddRange(enums);

                        var enumMembers = root.DescendantNodes()
                                             .OfType<EnumMemberDeclarationSyntax>()
                                             .Select(x => new Item(x.Identifier.ValueText, ItemType.EnumMember, x.Span))
                                             .ToList();

                        list.AddRange(enumMembers);

                        var structs = root.DescendantNodes()
                                             .OfType<StructDeclarationSyntax>()
                                             .Select(x => new Item(x.Identifier.ValueText, ItemType.Struct, x.Span))
                                             .ToList();

                        list.AddRange(structs);

                        var constructors = root.DescendantNodes()
                                               .OfType<ConstructorDeclarationSyntax>()
                                               .Select(x => new Item(x.Identifier.ValueText, ItemType.Constructor, x.Span))
                                               .ToList();

                        list.AddRange(constructors);

                        var methods = root.DescendantNodes()
                                          .OfType<MethodDeclarationSyntax>()
                                          .Select(x => new Item(x.Identifier.ValueText, ItemType.Method, x.Span))
                                          .ToList();

                        list.AddRange(methods);

                        var fields = root.DescendantNodes()
                                         .OfType<FieldDeclarationSyntax>()
                                         .SelectMany(x => x.Declaration.Variables,
                                                     (f, v) => f.Modifiers.Any(SyntaxKind.ConstKeyword) ? new Item(v.Identifier.ValueText, ItemType.Const, v.Span)
                                                                                                        : new Item(v.Identifier.ValueText, ItemType.Field, v.Span))
                                         .ToList();

                        list.AddRange(fields);

                        var properties = root.DescendantNodes()
                                             .OfType<PropertyDeclarationSyntax>()
                                             .Select(x => new Item(x.Identifier.ValueText, ItemType.Property, x.Span))
                                             .ToList();

                        list.AddRange(properties);

                        var eventFields = root.DescendantNodes()
                                              .OfType<EventFieldDeclarationSyntax>()
                                              .SelectMany(x => x.Declaration.Variables, (f, v) => new Item(v.Identifier.ValueText, ItemType.Event, v.Span))
                                              .ToList();

                        list.AddRange(eventFields);

                        var eventProperties = root.DescendantNodes()
                                                  .OfType<EventDeclarationSyntax>()
                                                  .Select(x => new Item(x.Identifier.ValueText, ItemType.Event, x.Span))
                                                  .ToList();

                        list.AddRange(eventProperties);

                        var delegates = root.DescendantNodes()
                                             .OfType<DelegateDeclarationSyntax>()
                                             .Select(x => new Item(x.Identifier.ValueText, ItemType.Delegate, x.Span))
                                             .ToList();

                        list.AddRange(delegates);

                        
                        //End of document/class
                    }

                    
                }

            }

            return RelationsGraph.BuildTree(list).Where(item => item.Type == ItemType.Method).ToList();
        }
    }
}
