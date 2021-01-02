using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using NET.Processor.Core.Interfaces;
using NET.Processor.Core.Models.API.Database;
using NET.Processor.Core.Models.RelationsGraph.Item;
using NET.Processor.Core.Models.RelationsGraph.Item.Base;

namespace NET.Processor.Core.Services.Database
{
    public class DatabaseService : IDatabaseService
    {
        private readonly string databaseString = "netProcessorDB";
        private readonly string databaseConnectionString = "mongodb+srv://admin_ben:6IePzWJHwqcPapWV@cluster0.zqe3a.mongodb.net/netProcessorDB?retryWrites=true";
        private readonly string projectsCollectionName = "Projects";
        private readonly string accountsCollectionName = "Accounts";
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

        public Task<GithubRepository> GetRepositoryToken(string repositoryType)
        {
            try
            {
                if (repositoryType is "github")
                {
                    // Get newly created collection from database based on project name
                    var collection = database.GetCollection<GithubRepository>(accountsCollectionName);
                    var result = collection.Find(x => x.github.profile.provider == repositoryType).ToList();
                    Console.WriteLine(result);
                    if (result.Count > 1)
                    {
                        // TODO: Throw exception is not working here, need to check why!
                        throw new Exception("GetRepositoryToken method returned more than one token, only one token is allowed");
                    }
                    return null;
                }
                else
                {
                    // TODO: Throw exception is not working here, need to check why!
                    throw new Exception("No repository type chosen, please specify a repository type");
                }
            } catch(Exception e)
            {
                // TODO: Throw exception is not working here, need to check why!
                throw new Exception($"Exception occurred during retrieving repository token, exception is: { e }");
            }
        }

        public void StoreGraphNodesAndEdges(ProjectRelationsGraph relationGraphNodesAndEdges)
        {
            try
            {
                if(database.GetCollection<Item>(projectsCollectionName) == null) {
                    database.CreateCollection(projectsCollectionName);
                }
                // Get newly created collection from database based on project name
                var collection = database.GetCollection<ProjectRelationsGraph>(projectsCollectionName);
                var result = collection.Find(x => x.SolutionName == relationGraphNodesAndEdges.SolutionName).ToList();

                // If Solution name cannot be found, insert collection directly, otherwise delete old one 
                // and insert new solution afterwards
                if(result.Count == 0)
                {
                    // Insert graph data into collection per project
                    collection.InsertOne(relationGraphNodesAndEdges);
                } else
                {
                    // Delete graph data first then create collection again per project
                    collection.DeleteOne(a => a.Id == result[0].Id);
                    collection.InsertOne(relationGraphNodesAndEdges);
                } 
                
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
