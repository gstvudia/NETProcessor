using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Build.Locator;
using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using NET.Processor.Core.Interfaces;
using NET.Processor.Core.Services;
using NET.Processor.Core.Models;

namespace NETProcessor.Main
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //setup our DI
            var serviceProvider = new ServiceCollection()
                .AddSingleton<ISolutionService, SolutionService>()
                .AddSingleton<IMethodService, MethodService>()
                .AddSingleton<ICommentService, CommentService>()
                .BuildServiceProvider();

            var _solutionService = serviceProvider.GetService<ISolutionService>();
            var _methodService = serviceProvider.GetService<IMethodService>();
            var _commentService = serviceProvider.GetService<ICommentService>();

            string workingDirectory = Environment.CurrentDirectory;
            string rootPath = Directory.GetParent(workingDirectory).Parent.Parent.FullName;
            // string projectPath = "/TestProject/nopCommerce-develop/src/NopCommerce.sln";
            //string projectPath = "/TestProject/TestProject/TestProject.sln";
            //string pathSolution = rootPath + projectPath; 

            //** REPO : https://github.com/ardalis/CleanArchitecture   ** //
            //string pathSolution = @"C:\Users\Gustavo Melo\Documents\BGDoc\EXAMPLS\CleanArchitecture-master\CleanArchitecture.sln";
            string pathSolution = "https://github.com/ardalis/CleanArchitecture/CleanArchitecture.sln";
            // Load solution information
            _solutionService.GetSolutionFromRepo(new WebHook() { RepositoryURL = "https://github.com/ardalis/CleanArchitecture/", User = "", Password = "" });
            //var solution = _solutionService.LoadSolution(pathSolution);
            // Load files of solution
            //var csharpCompileFileList = _solutionService.LoadFilePaths(pathSolution);



            //********METHODS RELATION GRAPH*****//
            //Gets all method inside solution and its references
            var references = new List<MethodReference>();

            //if (solution != null)
            //{
            //    _solutionService.GetSolutionItens(solution);
            //    var allmethods = _methodService.GetAllMethods(solution);
            //     
            //    foreach(var method in allmethods)
            //    {
            //        references.AddRange(_methodService.GetMethodReferencesByName(method.Name, solution).ToList());
            //    }
            //}            


            //********COMENTS*****//
            // Get all comments
            //var comments = _commentService.GetCommentReferences(csharpCompileFileList);
            
        }

        

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

    }
}
