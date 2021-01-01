using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using NET.Processor.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using NET.Processor.Core.Models;
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using NET.Processor.Core.Models.RelationsGraph.Item;
using Lib2Git = LibGit2Sharp;
using NET.Processor.Core.Helpers;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using DynamicData;
using Microsoft.Extensions.Configuration;
using NET.Processor.Core.Models.RelationsGraph.Item.Base;
using System.IO;

namespace NET.Processor.Core.Services.Project
{
    public class SolutionService : ISolutionService
    {
        private readonly IDatabaseService _databaseService;
        private readonly ISolutionGraph _solutionGraph;
        private readonly IConfiguration _configuration;

        private Solution solution = null;
        private readonly string path = null;

        public SolutionService(IDatabaseService databaseService, ISolutionGraph solutionGraph, IConfiguration configuration)
        {
            _solutionGraph = solutionGraph;
            _databaseService = databaseService;
            _configuration = configuration;
 
            path = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())), "repos");
            // Create directory if not existing, otherwise do nothing
            System.IO.Directory.CreateDirectory(path);

            _databaseService.ConnectDatabase();

            if (!MSBuildLocator.IsRegistered)
            {
                MSBuildLocator.RegisterDefaults();
            }

            // Automapper configuration
            //_automapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<Item, ItemDTO>()
            //    .ForMember(dest => dest.ParentId, act => act.MapFrom(src => src.Parent.Id))
            //);
        }

        /// <summary>
        /// Get repository to load solution from
        /// </summary>
        /// <param name="repositoryName"></param>
        /// <returns></returns>
        public void SaveSolutionFromRepository(CodeRepository repository)
        {
            // If path exists, remove old solution and add new one
            string solutionPath = DirectoryHelper.FindFileInDirectory(path, repository.SolutionFilename);
            if (solutionPath != null)
            {
                try
                {
                    DirectoryHelper.ForceDeleteReadOnlyDirectory(solutionPath);
                } catch (Exception e)
                {
                    throw new Exception(
                    $"There was an error deleting the project under the following path: { solutionPath }, the error was: { e } ");
                }
            }
            string repositoryPath = Path.Combine(path, repository.SolutionName);
            try
            {
                var cloneOptions = new Lib2Git.CloneOptions
                {
                    //CredentialsProvider = (_url, _user, _cred) => new UsernamePasswordCredentials { Username = repository.User, Password = repository.Password }
                    CredentialsProvider = (_url, _user, _cred) => new Lib2Git.UsernamePasswordCredentials { Username = repository.Token, Password = string.Empty }
                };
                Lib2Git.Repository.Clone(repository.RepositoryURL, repositoryPath, cloneOptions);
            } catch (Exception e)
            {
                if (e is Lib2Git.NameConflictException)
                {
                    throw new Lib2Git.NameConflictException(
                    $"There was an error cloning the project with the following repository URL: { repository.RepositoryURL }, the repository already exists");
                }
                else if (e is Lib2Git.NotFoundException)
                {
                    throw new Lib2Git.NotFoundException(
                    $"There was an error cloning the project with the following repository URL: { repository.RepositoryURL }, the repository specified could not be found");
                }
                else
                {
                    throw new Exception(
                        $"There was an error cloning the project with the following repository URL: { repository.RepositoryURL }, the error was: { e } ");
                }
            }
        }

        public async Task<Solution> LoadSolution(string solutionName, string solutionFilename)
        {
            string solutionPath = DirectoryHelper.FindFileInDirectory(path, solutionFilename);
            // If solution to process could not be found, throw exception
            if (solutionPath == null)
            {
                throw new Lib2Git.NotFoundException(
                    $"The specified solution under the solution path: { solutionPath } could not be found.");
            }

            // Solution base path "\\source\\repos\\Solutions\\ + {{ SolutionName }} + \\ {{ SolutionFilename }}.sln
            solutionPath = Path.Combine(solutionPath, solutionFilename + ".sln");

            using var msWorkspace = CreateMSBuildWorkspace();
            try
            {
                solution = await msWorkspace.OpenSolutionAsync(solutionPath);

                // TODO: We can log diagnosis later
                ImmutableList<WorkspaceDiagnostic> diagnostics = msWorkspace.Diagnostics;

            }
            catch (Exception e)
            {
                throw new Exception(
                    $"There was an error opening the project with the following path: { solutionPath }, the error was: { e } ");
            }

            return solution;
        }

        private MSBuildWorkspace CreateMSBuildWorkspace()
        {
            MSBuildWorkspace msWorkspace = null;

            try
            {
                msWorkspace = MSBuildWorkspace.Create();
            }
            catch (Exception e)
            {
                throw new Exception(
                    $"There was an error creating the MSBuildWorkspace, the error was: { e } ");
            }

            return msWorkspace;
        }

        public IEnumerable<Item> GetRelationsGraph(Solution solution)
        {
            return _solutionGraph.GetRelationsGraph(solution);
        }

        public void ProcessRelationsGraph(IEnumerable<Item> relations, string solutionName, string repositoryToken)
        {
            ProjectRelationsGraph relationGraph = _solutionGraph.ProcessRelationsGraph(relations, solutionName, repositoryToken).Result;
            // Store collection in Database
            _databaseService.StoreGraphNodesAndEdges(relationGraph);
            // Remark: No need to close db again, handled by database engine (MongoDB)
        }
    }
}
