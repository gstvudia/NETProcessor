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
using Microsoft.CodeAnalysis.FindSymbols;
using System.Linq.Expressions;
using static NET.Processor.Core.Services.CommentIdentifier;
using ReactiveUI;

namespace NET.Processor.Core.Services
{
    public class SolutionService : ISolutionService
    {
        public SolutionService()
        {
            if (!MSBuildLocator.IsRegistered)
            {
                MSBuildLocator.RegisterDefaults();
            }
        }

        public async Task<Solution> LoadSolution(string solutionPath)
        {
            Solution solution = null;
            using (var msWorkspace = MSBuildWorkspace.Create())
            {
                try
                {                    
                    solution = await msWorkspace.OpenSolutionAsync(solutionPath);

                    //TODO: We can log diagnosis later
                    ImmutableList<WorkspaceDiagnostic> diagnostics = msWorkspace.Diagnostics;

                }
                catch (Exception){}
            
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
                //Repository.Clone("https://github.com/ardalis/CleanArchitecture.git", Directory.GetParent(Directory.GetCurrentDirectory()).FullName + "/Clones");

                //get file names or something like that and add on the list
                //DirectoryFiles.Add();
            }
            catch (Exception ex)
            {
                Console.WriteLine();
            }
            

            return DirectoryFiles;
        }

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

        public IEnumerable<Item> GetSolutionItems(Solution solution)
        {
            SyntaxNode root = null;
            var list = new List<Item>();

            var methodsSheize = new List<Item>();
            var invocationsSheize = new List<InvocationExpressionSyntax>();

            List<RegionDirectiveTriviaSyntax> regionDirectives = null;
            List<EndRegionDirectiveTriviaSyntax> endRegionDirectives = null;
            EndRegionDirectiveTriviaSyntax endNode = null;
            int lastId = 0;

            foreach (var project in solution.Projects)
            {
                if (project.Language == "C#")
                {
                    foreach (var documentClass in project.Documents)
                    {
                        root = documentClass.GetSyntaxRootAsync().Result;

                        invocationsSheize.AddRange(root.DescendantNodes().OfType<InvocationExpressionSyntax>());

                        


                        MapMethods(root);
                    }
                    
                }

            }

                      
            return RelationsGraph.BuildTree(list.Where(item => item.Type == ItemType.Method).ToList());
        }

        private void MapMethods (SyntaxNode root)
        {
            var methodNodes = root.DescendantNodes().OfType<MethodDeclarationSyntax>();
            var methods = methodNodes
                                          .Select(x => new Item(root.DescendantNodes().IndexOf(x), x.Identifier.ValueText, x.Body))
                                          .ToList();
            foreach (var method in methods)
            {
                var invokees = method.Body.Statements.Where(x => x.Kind().ToString() == "ExpressionStatement").ToList();
                foreach (var invoked in invokees)
                {
                    var expression = invoked as ExpressionStatementSyntax;
                    var arguments = expression.Expression as InvocationExpressionSyntax;
                    var child = invoked.ToString().Replace(arguments.ArgumentList.ToString(), string.Empty).Replace(";", string.Empty).Split(".");

                    if (child.Length > 1)
                    {
                        var methodId = MapChildsId(methodNodes.ToList(), child[1]);
                        if (methodId > -1)
                        {
                            method.ChildList.Add(new Item(methodId, child[1]));
                        }

                    }
                    else
                    {
                        var methodId = MapChildsId(methodNodes.ToList(), child[0]);
                        if (methodId > -1)
                        {
                            method.ChildList.Add(new Item(methodId, child[0]));
                        }
                    }
                }

            }

        }
        private int MapChildsId(List<MethodDeclarationSyntax> nodes, string method)
        {
            
            return nodes.IndexOf(nodes.Where(x => x.Identifier.ValueText == method).Select(x => x).FirstOrDefault());
        }


    }
}
