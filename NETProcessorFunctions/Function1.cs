using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NET.Processor.Core.Models;
using NET.Processor.Core.Interfaces;
using NET.Processor.Core.Services;

namespace NETProcessorFunctions
{
    public class Function1
    {
        [FunctionName("Test")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string responseMessage = "Benny is the enw Gustavo";
            SolutionService service = new SolutionService();
            string path = @"C:\Users\Gustavo Melo\Documents\BGDoc\EXAMPLS\CleanArchitecture-master\CleanArchitecture.sln";
            //string path = "/src/Solutions/CleanArchitecture-master/CleanArchitecture.sln";
            var solution = service.LoadSolution(path);
            service.GetSolutionItems(solution);

            return new OkObjectResult(responseMessage);
            //log.LogInformation("C# HTTP trigger function processed a request.");
            //
            //string name = req.Query["name"];
            //
            //string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            //dynamic data = JsonConvert.DeserializeObject(requestBody);
            //name = name ?? data?.name;
            //
            //string responseMessage = string.IsNullOrEmpty(name)
            //    ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
            //    : $"Hello, {name}. This HTTP triggered function executed successfully.";

        }
    }
}
