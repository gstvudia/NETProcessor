using System;
using System.IO;

namespace NET.Processor.Core.Helpers
{
    public class DirectoryHelper
    {
        public static string FindFileInDirectory(string basePath, string filename)
        {
            try
            {
                foreach (string directory in Directory.GetDirectories(basePath))
                {
                    foreach (string file in Directory.GetFiles(directory, filename + ".sln"))
                    {
                        return directory;
                    }
                    // FindPathOfSolution(directory); // Recursive folder checking not needed as of now
                }
            }
            catch (Exception e)
            {
                throw new Exception(
                    $"There was an error during finding the file: { filename } in the base path: { basePath }, the error was: { e } ");
            }
            return null;
        }

        public static void ForceDeleteReadOnlyDirectory(string path)
        {
            var directory = new DirectoryInfo(path) { Attributes = FileAttributes.Normal };

            foreach (var info in directory.GetFileSystemInfos("*", SearchOption.AllDirectories))
            {
                info.Attributes = FileAttributes.Normal;
            }

            directory.Delete(true);
        }
    }
}
