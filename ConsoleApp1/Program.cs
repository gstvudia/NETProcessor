using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            string methodName = "ProcessPayment";
            string solutionPath = @"C:\Users\Gustavo Melo\Documents\BGDoc\EXAMPLS\nopCommerce-develop\nopCommerce-develop\src\NopCommerce.sln";
            // AnalyzerManager manager = new AnalyzerManager();
            // ProjectAnalyzer analyzer = (ProjectAnalyzer)manager.GetProject(@"C:\MyCode\MyProject.csproj");
            // AdhocWorkspace workspace = analyzer.GetWorkspace();
            var msWorkspace = MSBuildWorkspace.Create();
            List<ReferencedSymbol> referencesToMethod = new List<ReferencedSymbol>();
            Console.WriteLine("Searching for method \"{0}\" reference in solution {1} ", methodName, Path.GetFileName(solutionPath));
            ISymbol methodSymbol = null;
            bool found = false;

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
