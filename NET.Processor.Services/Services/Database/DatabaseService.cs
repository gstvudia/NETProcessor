using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using NET.Processor.Core.Interfaces;
using NET.Processor.Core.Models.RelationsGraph.Item;

namespace NET.Processor.Core.Services.Database
{
    public class DatabaseService : IDatabaseService
    {
        private string databaseString = "netProcessorDB";
        private string databaseConnectionString = "mongodb+srv://admin_ben:6IePzWJHwqcPapWV@cluster0.zqe3a.mongodb.net/netProcessorDB?retryWrites=true&w=majority";
        private IMongoClient client;
        private IMongoDatabase database;
        public IMongoCollection<Item> item_collection { get; set; }

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

        public void StoreCollection(string solutionName, IEnumerable<Item> itemList)
        {
            try
            {
                // Create collection name corresponding to project name
                database.CreateCollection(solutionName);
                // Get newly created collection from database based on project name
                var collection = database.GetCollection<Item>(solutionName);
                // Insert graph data into collection per project
                collection.InsertMany(itemList);
            } catch(Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public IEnumerable<Item> GetCollection(string solutionName)
        {
            // Projection builder
            var projectionBuilder = Builders<Item>.Projection;
            var projection = projectionBuilder.Include(x => x.Id).Include(x => x.Name).Include(u => u.Span);
            // Filter builder
            var filterBuilder = Builders<Item>.Filter;
            var filter = filterBuilder.Empty;

            try
            {
                // Get collection based on project name
                var collection = database.GetCollection<Item>(solutionName);
                // Get cursor based on projection and filter builder
                var itemCollections = collection.Find(filter).Project(projection).ToList();
                // Convert what cursor found to List<Item> itemList
                return null;
            } catch(Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
