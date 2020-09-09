using Microsoft.Build.Locator;
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

namespace NET.Processor.Core.Services
{
    public class MethodService : IMethodService
    {

        public MethodService()
        {

        }

        public List<Method> GetAllMethods(Solution solution)
        {
            List<Method> Methods = new List<Method>();

            foreach (var project in solution.Projects)
            {
                //TODO: THINK OF A BETTER SOLUTION FOR THIS
                //DO A UNIFIER FOR VB AND C#, TREAT .NET AS UNIQUE
                if(project.Language == "C#") {
                    foreach (var documentClass in project.Documents)
                    { 
                        var model = documentClass.GetSemanticModelAsync().Result;
                        var methodInvocation = documentClass.GetSyntaxRootAsync().Result;
                        var IsInterface = methodInvocation.DescendantNodes().OfType<InterfaceDeclarationSyntax>();

                        //to make sure it's a class or a struct, not an interface        
                        if(!IsInterface.Any())
                        {
                            try
                            { //meu

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
                            catch (Exception exception){}
                        }
                    }
                }                    
            }
            
            return Methods;
        }


        public void GetMethodReferencesByName(string method,Solution solution)
        {
            //List<ReferencedSymbol> referencesToMethod = new List<ReferencedSymbol>();
            //ISymbol methodSymbol = null;
            //List<ReferenceLocation> allReferences = new List<ReferenceLocation>();
            //
            //
            //foreach (var project in solution.Projects)
            //{
            //    //TODO: THINK OF A BETTER SOLUTION FOR THIS
            //    //DO A UNIFIER FOR VB AND C#, TREAT .NET AS UNIQUE
            //    if (project.Language == "C#")
            //    {
            //        foreach (var document in project.Documents)
            //        {
            //
            //            var model = document.GetSemanticModelAsync().Result;
            //
            //            var methodInvocation = document.GetSyntaxRootAsync().Result;
            //            //meu
            //            var documentIsClass = model.GetType().IsClass;
            //            var IsInterface = methodInvocation.DescendantNodes().OfType<InterfaceDeclarationSyntax>();
            //            //
            //            InvocationExpressionSyntax node = null;
            //
            //            //to make sure it's a class or a struct, not an interface
            //
            //            if (!IsInterface.Any())
            //            {
            //                try
            //                { //meu
            //
            //                    var members = methodInvocation.DescendantNodes().OfType<MemberDeclarationSyntax>().Distinct();
            //
            //                    foreach (var member in members)
            //                    {
            //                        var method = member as MethodDeclarationSyntax;
            //                        if (method != null)
            //                        {
            //                            Console.WriteLine("Class: " + document.Name + " - - - Method: " + method.Identifier.Text);
            //                        }
            //
            //                    }
            //                    //aaaaa
            //                    //node = methodInvocation.DescendantNodes().OfType<InvocationExpressionSyntax>()
            //                    // .Where(x => ((MemberAccessExpressionSyntax)x.Expression).Name.ToString() == methodName).FirstOrDefault();
            //                    //
            //                    //if (node == null)
            //                    //    continue;
            //                }
            //                catch (Exception exception)
            //                {
            //                    // Swallow the exception of type cast. 
            //                    // Could be avoided by a better filtering on above linq.
            //                    continue;
            //                }
            //            }
            //
            //
            //            //methodSymbol = model.GetSymbolInfo(node).Symbol;
            //            //break;
            //        }
            //    }
            //}
            //
            ////foreach (var item in SymbolFinder.FindReferencesAsync(methodSymbol, solution).Result)
            ////{
            ////    foreach (var location in item.Locations)
            ////    {
            ////        allReferences.Add(location);
            ////    }
            ////    
            ////}
            //
            //return allReferences;
        }
    }


}
