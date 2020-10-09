using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using NET.Processor.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Build.Evaluation;
using System.Threading.Tasks;

namespace NET.Processor.Core.Services
{
    public class SolutionService : ISolutionService
    {

        public SolutionService()
        {

        }

        public Solution LoadSolution(string SolutionPath)
        {
            Solution solution = null;

            MSBuildLocator.RegisterDefaults();

            using (var msWorkspace = MSBuildWorkspace.Create())
            {
                try
                {
                    solution = msWorkspace.OpenSolutionAsync(SolutionPath).Result;
                }
                catch (Exception)
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

        public IEnumerable<FileInfo> LoadFilePaths(string solutionFilePath)
        {
            List<FileInfo> cSharpCompileFileList = new List<FileInfo>();

            foreach (var csharpCompileFile in GetProjectFilesForSolution(new FileInfo(solutionFilePath)).SelectMany(projectFile => GetCSharpCompileItemFilesForProject(projectFile)))
            {
                cSharpCompileFileList.Add(csharpCompileFile);
            }

            return cSharpCompileFileList;
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

        private static IEnumerable<FileInfo> GetCSharpCompileItemFilesForProject(FileInfo projectFile)
        {
            if (projectFile == null)
                throw new ArgumentNullException("projectFile");

            return (new ProjectCollection()).LoadProject(projectFile.FullName).AllEvaluatedItems
                .Where(item => item.ItemType == "Compile")
                .Select(item => item.EvaluatedInclude)
                .Where(include => include.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                .Select(include => new FileInfo(Path.Combine(projectFile.Directory.FullName, include)));
        }
    }
}
