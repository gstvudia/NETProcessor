using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.MSBuild;
using NET.Processor.Core.Interfaces;
using NET.Processor.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using VSSolution = Microsoft.CodeAnalysis.Solution;

namespace NET.Processor.Core.Services
{
    public class MethodService : IMethodService
    {
        private readonly IConfiguration _configuration;

        public MethodService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /*
        public IEnumerable<Method> GetAllMethods(Solution solution)
        {
            List<Method> Methods = new List<Method>();

            foreach (var project in solution.Projects)
            {
                //TODO: THINK OF A BETTER SOLUTION FOR THIS
                //DO A UNIFIER FOR VB AND C#, TREAT .NET AS UNIQUE
                if(project.Language == _configuration["Framework:Language"]) {
                    foreach (var documentClass in project.Documents)
                    {
                        var model = documentClass.GetSemanticModelAsync().Result;
                        var methodInvocation = documentClass.GetSyntaxRootAsync().Result;
                        var IsInterface = methodInvocation.DescendantNodes().OfType<InterfaceDeclarationSyntax>();

                        //to make sure it's a class or a struct, not an interface        
                        if(!IsInterface.Any())
                        {
                            try
                            {

                                var members = methodInvocation.DescendantNodes().OfType<MemberDeclarationSyntax>().Distinct();

                                foreach (var member in members)
                                {
                                    var method = member as MethodDeclarationSyntax;
                                    if (method != null)
                                    {
                                        Methods.Add( new Method { Class = documentClass.Name, Name = method.Identifier.Text});
                                    }
                                }
                            }
                            catch (Exception exception){
                                Console.WriteLine(exception);
                            }
                        }
                    }
                }                    
            }
            
            return Methods;
        }
        */

        
        public IEnumerable<MethodReference> GetMethodReferencesByName(string methodName, VSSolution solution)
        {
            List<ReferencedSymbol> referencesToMethod = new List<ReferencedSymbol>();
            ISymbol methodSymbol = null;
            List<MethodReference> callingMethods = new List<MethodReference>();


            foreach (var project in solution.Projects)
            {
                //TODO: THINK OF A BETTER SOLUTION FOR THIS
                //DO A UNIFIER FOR VB AND C#, TREAT .NET AS UNIQUE
                if (project.Language == _configuration["Framework:Language"])
                {
                    foreach (var document in project.Documents)
                    {
            
                        var model = document.GetSemanticModelAsync().Result;
            
                        var methodInvocation = document.GetSyntaxRootAsync().Result;
                        InvocationExpressionSyntax node = null;

                        //to make sure it's a class or a struct, not an interface
                        
                            try
                            { 
                                node = methodInvocation.DescendantNodes().OfType<InvocationExpressionSyntax>()
                                 .Where(x => ((MemberAccessExpressionSyntax)x.Expression).Name.ToString() == methodName).FirstOrDefault();
                                
                                if (node == null)
                                    continue;
                            }
                            catch (Exception exception)
                            {
                                Console.WriteLine(exception.Message);
                                // Swallow the exception of type cast. 
                                // Could be avoided by a better filtering on above linq.
                                continue;
                            }


                        methodSymbol = model.GetSymbolInfo(node).Symbol;
                        break;
                    }
                }
            }

            foreach (var item in SymbolFinder.FindReferencesAsync(methodSymbol, solution).Result)
            {
                foreach (var location in item.Locations)
                {
                    int spanStart = location.Location.SourceSpan.Start;
                    var doc = location.Document;

                    var indexerInvokation = doc.GetSyntaxRootAsync().Result
                        .DescendantNodes()
                        .FirstOrDefault(node => node.GetLocation().SourceSpan.Start == spanStart);

                    var methodCalling = indexerInvokation.Ancestors()
                        .OfType<MethodDeclarationSyntax>()
                        .FirstOrDefault()
                        ?.Identifier.Text ?? String.Empty;


                    var className = indexerInvokation.Ancestors()
                        .OfType<ClassDeclarationSyntax>()
                        .FirstOrDefault()
                        ?.Identifier.Text ?? String.Empty;

                    callingMethods.Add(new MethodReference { MethodCallerClass = className, MethodCaller = methodCalling, MethodCalled = methodName });
                }                
            }

            return callingMethods;
        }
    }


}
