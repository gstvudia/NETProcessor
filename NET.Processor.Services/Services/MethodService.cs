using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.MSBuild;
using NET.Processor.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NET.Processor.Core.Services
{
    public class MethodService : IMethodService
    {

        public MethodService()
        {

        }

        public IEnumerable<ReferenceLocation> GetMethodReferences(string methodName, Solution solution)
        {
            List<ReferencedSymbol> referencesToMethod = new List<ReferencedSymbol>();
            ISymbol methodSymbol = null;
            IEnumerable<ReferenceLocation> allReferences = null;

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
                    break;
                }

            }

            foreach (var item in SymbolFinder.FindReferencesAsync(methodSymbol, solution).Result)
            {
                allReferences = item.Locations;
            }

            return allReferences;
        }
    }
}
