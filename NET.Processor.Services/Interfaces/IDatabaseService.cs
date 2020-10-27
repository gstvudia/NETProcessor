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
        /// Store collection in Database
        /// </summary>
        void StoreCollection(string solutionName, Root relationGraph);

        /// <summary>
        /// Store test collection in Database
        /// </summary>
        /// <param name="solutionName"></param>
        /// <param name="listItems"></param>
        void StoreCollectionTest(string solutionName, List<Item> listItems);

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
