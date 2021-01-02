using NET.Processor.Core.Models.API;
using NET.Processor.Core.Models.API.Database;
using System.Text.Json;
using System.Threading.Tasks;

namespace NET.Processor.Core.Interfaces
{
    public interface IGithubService
    {
        /// <summary>
        /// Getting Github repository token from Database
        /// </summary>
        /// <returns>Github repository token</returns>
        Task<string> GetToken(string repositoryType);
        /// <summary>
        /// Gets the github repository link to the file in which the method resides
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="fileName"></param>
        /// <param name="repositoryOwner"></param>
        /// <param name="solutionName"></param>
        /// <returns>url link to the file in which method resides</returns>
        Task<GithubJSONResponse.Root> GetLinkToMethod(string methodName, string fileName, string repositoryOwner, string solutionName);
    }
}
