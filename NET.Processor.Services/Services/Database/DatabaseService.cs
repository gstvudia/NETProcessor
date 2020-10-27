using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using NET.Processor.Core.Interfaces;
using NET.Processor.Core.Models.RelationsGraph.Item;
using NET.Processor.Core.Models.RelationsGraph.Item.Base;

namespace NET.Processor.Core.Services.Database
{
    public class DatabaseService : IDatabaseService
    {
        private string databaseString = "netProcessorDB";
        private string databaseConnectionString = "mongodb+srv://admin_ben:6IePzWJHwqcPapWV@cluster0.zqe3a.mongodb.net/netProcessorDB?retryWrites=true&w=majority";
        private IMongoClient client;
        private IMongoDatabase database;

        public void ConnectDatabase()
        {
            if (client == null)
            {
                try
                {
                    client = new MongoClient(databaseConnectionString);
                    database = client.GetDatabase(databaseString);
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
        }

        public void StoreCollectionTest(string solutionName, List<Item> listItems)
        {
            try
            {
                // If project exists, remove it and recreate it in the database
                if (database.GetCollection<Item>(solutionName) != null)
                {
                    database.DropCollection(solutionName);
                }
                // Create collection name corresponding to project name
                database.CreateCollection(solutionName);
                // Get newly created collection from database based on project name
                var collection = database.GetCollection<Item>(solutionName);
                // Insert graph data into collection per project
                collection.InsertMany(listItems);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public void StoreCollection(string solutionName, Root relationGraph)
        {
            try
            {
                // If project exists, remove it and recreate it in the database
                if(database.GetCollection<Item>(solutionName) != null)
                {
                    database.DropCollection(solutionName);
                }
                // Create collection name corresponding to project name
                database.CreateCollection(solutionName);
                // Get newly created collection from database based on project name
                var collection = database.GetCollection<Root>(solutionName);
                // Insert graph data into collection per project
                collection.InsertOne(relationGraph);
               
            } catch(Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        // Not used, data is retrieved directly through frontend from mongodb instead!
        /*
        public async Task<IEnumerable<Item>> GetCollection(string solutionName)
        {
            try
            {
                // Get collection based on project name
                var collection = database.GetCollection<Item>(solutionName);
                List<Item> itemList = null;
                // Get cursor based on projection and filter builder
                // await collection.Find(new BsonDocument()).ForEachAsync(x => itemList.Add()));
                return itemList;
            } catch(Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        */
    }
}
