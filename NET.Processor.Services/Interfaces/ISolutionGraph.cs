using NET.Processor.Core.Models;
using NET.Processor.Core.Models.RelationsGraph.Item.Base;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NET.Processor.Core.Services.Project
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
        Task<ProjectRelationsGraph> ProcessRelationsGraph(IEnumerable<Method> relations, string solutionName, string repositoryToken);
    }
}