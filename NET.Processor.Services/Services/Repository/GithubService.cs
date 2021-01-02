using NET.Processor.Core.Interfaces;
using NET.Processor.Core.Models.API;
using NET.Processor.Core.Models.API.Database;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace NET.Processor.Core.Services.Repository
{
    public class GithubService : IGithubService
    {
        private static readonly string BaseUrl = "https://api.github.com";
        private static readonly string BaseUserAgentProductName = "AppName";
        private static readonly string BaseUserAgentProductVersion = "1.0";
        private static readonly string RequestDataType = "application/json";
        private static readonly string GithubRepository = "github";

        private readonly IDatabaseService _databaseService;

        public GithubService(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<string> GetToken(string repositoryType)
        {
            GithubRepository repository = await _databaseService.GetRepositoryToken(repositoryType);
            return repository.github.token;
        }

        public async Task<GithubJSONResponse.Root> GetLinkToMethod(string methodName, string fileName, string repositoryOwner, string solutionName)
        {
            // TODO: Get Token from Database instead of as static string!
            //string token = await GetToken(GithubRepository);
            var client = HttpClient("f62ad638c32e15c4914b39b5cf3e4155a99f969b");
            var requestString = "/search/code?q=" + methodName + "+filename:" + fileName + "+repo:" + repositoryOwner + "/" + solutionName;
            try
            {   // When calling the file ending is NOT needed for the search e.g. filename.cs 
                HttpResponseMessage response = await client.GetAsync(requestString);
                if (response.IsSuccessStatusCode)
                {
                    
                    var result = await response.Content.ReadAsAsync<GithubJSONResponse.Root>();
                    if(result.items.Length > 1)
                    {
                        throw new Exception($"The request: { requestString } retrieved more than one url file location for the method in question, only one url file location is allowed");
                    }
                    return result;
                } 
                else
                {
                    throw new Exception($"The request: { requestString } was not successful, it returned with StatusCode: { response.StatusCode }");
                }
            }
            catch (Exception e)
            {
                throw new Exception(
                    $"The request: { requestString } was not successful, the error was: { e } ");
            }
        }

        private HttpClient HttpClient(string token)
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri(BaseUrl)
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(BaseUserAgentProductName, BaseUserAgentProductVersion));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(RequestDataType));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", token);
            return client;
        }
    }
}
