using Microsoft.Extensions.Hosting;
using NET.Processor.Core.Models;
using NET.Processor.Core.Models.RelationsGraph.Item;
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
        IEnumerable<Item> GetRelationsGraph(Microsoft.CodeAnalysis.Solution solution);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="relations"></param>
        /// <param name="solutionName"></param>
        Task<ProjectRelationsGraph> ProcessRelationsGraph(IEnumerable<Item> relations, string solutionName, string repositoryToken);
    }
}