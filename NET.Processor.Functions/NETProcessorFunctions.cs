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
using System.Collections.Generic;
using NET.Processor.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using NET.Processor.Core.Services;
using System.Linq;

namespace NET.Processor.Functions
{
    public class NETProcessorFunctions
    {
        private readonly ServiceProvider serviceProvider = null;
        private readonly SolutionService solutionService = null;
        private readonly MethodService methodService = null;
        private readonly CommentService commentService = null;
        //** REPO : https://github.com/ardalis/CleanArchitecture   ** //
        private static readonly string pathSolution = @"C:\Users\Benny\source\repos\CleanArchitecture\CleanArchitecture.sln";

        public NETProcessorFunctions()
        {
            //setup our DI
            serviceProvider = new ServiceCollection()
                .AddSingleton<ISolutionService, SolutionService>()
                .AddSingleton<IMethodService, MethodService>()
                .AddSingleton<ICommentService, CommentService>()
                .BuildServiceProvider();

            solutionService = (SolutionService) serviceProvider.GetService<ISolutionService>();
            methodService = (MethodService) serviceProvider.GetService<IMethodService>();
            commentService = (CommentService) serviceProvider.GetService<ICommentService>();

            string workingDirectory = Environment.CurrentDirectory;
            /*
                string rootPath = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
                string projectPath = "/TestProject/nopCommerce-develop/src/NopCommerce.sln";
                string projectPath = "/TestProject/TestProject/TestProject.sln";
                string pathSolution = rootPath + projectPath; 
            */
        }

        [FunctionName("Test")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            // Load solution information
            var service = new SolutionService();
            var solution = service.LoadSolution(pathSolution);
            // Load files of solution
            //var csharpCompileFileList = _solutionService.LoadFilePaths(pathSolution);



            //********METHODS RELATION GRAPH*****//
            //Gets all method inside solution and its references
            var references = new List<MethodReference>();

            if (solution != null)
            {
                var methodService = new MethodService();
                var allmethods = methodService.GetAllMethods(solution);

                foreach (var method in allmethods)
                {
                    references.AddRange(methodService.GetMethodReferencesByName(method.Name, solution).ToList());
                }
            }


            //********COMENTS*****//
            // Get all comments
            // var comments = _commentService.GetCommentReferences(csharpCompileFileList);

        



        //static void DiscoverProperties()
        //{
        //    var me = Assembly.GetExecutingAssembly().Location;
        //    var dir = Path.GetDirectoryName(me);
        //    var theClasses = @"C:\Users\Gustavo Melo\Documents\BGDoc\NETProcessor\NETProcessor.Main\DLL\Nop.Core.dll";
        //    
        //
        //    var assembly = Assembly.LoadFrom(theClasses);
        //    var types = assembly.GetTypes().ToList();
        //
        //    Type myType = assembly.GetTypes()[79];
        //    MethodInfo Method = myType.GetMethod("get_PickupFee");
        //    object myInstance = Activator.CreateInstance(myType);
        //    object threadexecMethod = Method.Invoke(myInstance, null);
        //
        //    var stack = new StackTrace();
        //    Method.Invoke(myInstance, null);
        //    var callerAssemblies = new StackTrace().GetFrames().ToList();
        //
        //    int propCount = 0 ;
        //    string propertiesList= string.Empty;
        //    string cName;
        //    string tempString;
        //    //ProjectInfoProvider decomp = new ProjectInfoProvider();
        //
        //    //decomp.DecompileProject
        //    try
        //    {
        //        foreach (var t in types)
        //        {
        //            
        //            cName = t.Name;
        //
        //            foreach (var prop in t.GetProperties())
        //            {
        //                propCount++;
        //                tempString = $"{prop.Name}:{prop.PropertyType.Name} ";
        //                propertiesList = propertiesList += tempString;
        //            }
        //        }
        //    }
        //    catch(Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //    }
        //    
        //
        //    Console.WriteLine(propertiesList);
        //}

            return new OkObjectResult(responseMessage);
        }
    }
}
