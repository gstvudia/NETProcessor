using Microsoft.CodeAnalysis;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using NET.Processor.Core.Models;
using NET.Processor.Core.Services;
using NET.Processor.Core.Models.RelationsGraph.Item;
using MongoDB.Driver;

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
        void StoreCollection(string solutionName, IEnumerable<Item> itemList);

        /// <summary>
        /// Gets the Graph Items of the Solution specified
        /// </summary>
        /// <param name="solutionName"></param>
        /// <returns>Graph Items of Solution</returns>
        IEnumerable<Item> GetCollection(string solutionName);

        // Remark, database does not need a disconnect method as it is handled
        // by the current Database System automatically (MongoDB)
    }
}
