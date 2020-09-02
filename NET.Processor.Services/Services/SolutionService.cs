using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.MSBuild;
using NET.Processor.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading.Tasks;

namespace NET.Processor.Core.Services
{
    class SolutionService : ISolutionService
    {

        public SolutionService()
        {

        }

        async Task<Solution> LoadSolutionAsync(string SolutionPath)
        {
            // @"C:\Users\Gustavo Melo\Documents\BGDoc\EXAMPLS\nopCommerce-develop\nopCommerce-develop\src\NopCommerce.sln";

            var solution = new Solution;

            MSBuildLocator.RegisterDefaults();

            using (var msWorkspace = MSBuildWorkspace.Create())
            {
                try
                {
                    solution = msWorkspace.OpenSolutionAsync(SolutionPath).Result;
                }
                catch(Exception)
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
    }
}
