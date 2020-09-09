using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Build.Locator;
using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using NET.Processor.Core.Interfaces;
using NET.Processor.Core.Services;

namespace NETProcessor.Main
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //setup our DI
            var serviceProvider = new ServiceCollection()
                .AddSingleton<ISolutionService, SolutionService>()
                .AddSingleton<IMethodService, MethodService>()
                .AddSingleton<ICommentService, CommentService>()
                .BuildServiceProvider();

            var _solutionService = serviceProvider.GetService<ISolutionService>();
            var _methodService = serviceProvider.GetService<IMethodService>();
            var _commentService = serviceProvider.GetService<ICommentService>();

            string workingDirectory = Environment.CurrentDirectory;
            string rootPath = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
            // string projectPath = "/TestProject/nopCommerce-develop/src/NopCommerce.sln";
            string projectPath = "/TestProject/TestProject/TestProject.sln";
            string pathSolution = rootPath + projectPath;
            // Load solution information
            var solution = _solutionService.LoadSolution(pathSolution);
            // Load files of solution
            var csharpCompileFileList = _solutionService.LoadFilePaths(pathSolution);

            //Gets all method references
            // var references = _methodService.GetMethodReferences("ProcessPayment",  solution);
            // Get all comments
            var comments = _commentService.GetCommentReferences(csharpCompileFileList);
            var references = _methodService.GetAllMethods( solution);
        }

        

        //static void DiscoverProperties()
        //{
        //    var me = Assembly.GetExecutingAssembly().Location;
        //    var dir = Path.GetDirectoryName(me);
        //    var theClasses = @"C:\Users\Gustavo Melo\Documents\BGDoc\NETProcessor\NETProcessor.Main\DLL\Nop.Core.dll";
        //    
        //
        //    var assembly = Assembly.LoadFrom(theClasses);
        //    var types = assembly.GetTypes().ToList();
        //
        //    Type myType = assembly.GetTypes()[79];
        //    MethodInfo Method = myType.GetMethod("get_PickupFee");
        //    object myInstance = Activator.CreateInstance(myType);
        //    object threadexecMethod = Method.Invoke(myInstance, null);
        //
        //    var stack = new StackTrace();
        //    Method.Invoke(myInstance, null);
        //    var callerAssemblies = new StackTrace().GetFrames().ToList();
        //
        //    int propCount = 0 ;
        //    string propertiesList= string.Empty;
        //    string cName;
        //    string tempString;
        //    //ProjectInfoProvider decomp = new ProjectInfoProvider();
        //
        //    //decomp.DecompileProject
        //    try
        //    {
        //        foreach (var t in types)
        //        {
        //            
        //            cName = t.Name;
        //
        //            foreach (var prop in t.GetProperties())
        //            {
        //                propCount++;
        //                tempString = $"{prop.Name}:{prop.PropertyType.Name} ";
        //                propertiesList = propertiesList += tempString;
        //            }
        //        }
        //    }
        //    catch(Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //    }
        //    
        //
        //    Console.WriteLine(propertiesList);
        //}

        public static void Roslyn()
        {
            string methodName = "ProcessPayment";
            string solutionPath = @"C:\Users\Gustavo Melo\Documents\BGDoc\EXAMPLS\nopCommerce-develop\nopCommerce-develop\src\NopCommerce.sln";
            //string solutionPath = @"C:\Users\Gustavo Melo\Documents\BGDoc\AnalyzingSourceCodeUsingRoslyn-master\AnalyzingSourceCodeUsingRoslyn.sln";
            // AnalyzerManager manager = new AnalyzerManager();
            // ProjectAnalyzer analyzer = (ProjectAnalyzer)manager.GetProject(@"C:\MyCode\MyProject.csproj");
            // AdhocWorkspace workspace = analyzer.GetWorkspace();
            MSBuildLocator.RegisterDefaults();

            using (var msWorkspace = MSBuildWorkspace.Create())
            {
                List<ReferencedSymbol> referencesToMethod = new List<ReferencedSymbol>();
                Console.WriteLine("Searching for method \"{0}\" reference in solution {1} ", methodName, Path.GetFileName(solutionPath));
                ISymbol methodSymbol = null;
                bool found = false;

                //Add await here
                var solution = msWorkspace.OpenSolutionAsync(solutionPath).Result;

                var teste = solution.Projects.FirstOrDefault(m => m.HasDocuments == true);
                ImmutableList<WorkspaceDiagnostic> diagnostics = msWorkspace.Diagnostics;
                foreach (var diagnostic in diagnostics)
                {
                    Console.WriteLine(diagnostic.Message);
                }



                foreach (var project in solution.Projects)
                {
   
                    foreach (var document in project.Documents)
                    {
                        var model = document.GetSemanticModelAsync().Result;

                        var methodInvocation = document.GetSyntaxRootAsync().Result;
                        InvocationExpressionSyntax node = null;
                        try
                        {
                            node = methodInvocation.DescendantNodes().OfType<InvocationExpressionSyntax>()
                             .Where(x => ((MemberAccessExpressionSyntax)x.Expression).Name.ToString() == methodName).FirstOrDefault();

                            if (node == null)
                                continue;
                        }
                        catch (Exception exception)
                        {
                            // Swallow the exception of type cast. 
                            // Could be avoided by a better filtering on above linq.
                            continue;
                        }

                        methodSymbol = model.GetSymbolInfo(node).Symbol;
                        found = true;
                        break;
                    }

                    if (found) break;
                }
                var teststst = SymbolFinder.FindReferencesAsync(methodSymbol, solution);
                foreach (var item in SymbolFinder.FindReferencesAsync(methodSymbol, solution).Result)
                {
                    foreach (var location in item.Locations)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Project Assembly -> {0}", location.Document.Project.AssemblyName);
                        Console.ResetColor();
                    }

                }

            }
            
        }
    }
}
