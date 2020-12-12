using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NET.Processor.Core.Models.RelationsGraph.Item;
using NET.Processor.Core.Models.RelationsGraph.Item.Base;

namespace NET.Processor.Core.Interfaces
{
    public interface IDatabaseService
    {
        /// <summary>
        /// Connect Database
        /// </summary>
        void ConnectDatabase();

        /// <summary>
        /// Store Graph Data (Nodes and Edges) in Database
        /// </summary>
        void StoreGraphNodesAndEdges(ProjectRelationsGraph relationGraph);
        /// <summary>
        /// Gets the Graph Items of the Solution specified
        /// </summary>
        /// <param name="solutionName"></param>
        /// <returns>Graph Items of Solution</returns>
        // Task<IEnumerable<Item>> GetCollection(string solutionName);

        // Remark, database does not need a disconnect method as it is handled
        // by the current Database System automatically (MongoDB)
    }
}
