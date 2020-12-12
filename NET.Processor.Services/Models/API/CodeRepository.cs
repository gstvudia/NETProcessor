namespace NET.Processor.Core.Models
{
    public class CodeRepository
    {
        public string RepositoryURL { get; set; }
        public string SolutionName { get; set; }
        public string SolutionFilename { get; set; }
        public string Token { get; set; }
        // For testing only Username and Password access to Repository 
        public string User { get; set; }
        public string Password { get; set; }
    }
}
