using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using NET.Processor.Core.Models;
using System.Collections.Generic;

namespace NET.Processor.Core.Interfaces
{
    public interface IMethodService
    {
        /// <summary>
        /// Finds all references for a method inside the solution
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="projects"></param>
        /// <returns>Projects</returns>
        // IEnumerable<Method> GetAllMethods(Solution solution);
        IEnumerable<MethodReference> GetMethodReferencesByName(string methodName, Solution solution);
    }
}
