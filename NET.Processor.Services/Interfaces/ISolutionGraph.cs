using NET.Processor.Core.Models;
using NET.Processor.Core.Models.RelationsGraph.Item.Base;
using System.Collections.Generic;

namespace NET.Processor.Core.Services.Solution
{
    public interface ISolutionGraph
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="solution"></param>
        /// <returns></returns>
        IEnumerable<Method> GetRelationsGraph(Microsoft.CodeAnalysis.Solution solution);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="relations"></param>
        /// <param name="solutionName"></param>
        ProjectRelationsGraph ProcessRelationsGraph(IEnumerable<Method> relations, string solutionName);
    }
}