using System;
using System.IO;
using System.Linq;

namespace ExternalProcessFunctions.Services
{
    public class TemporaryStorageManager
    {
        private readonly string dependencyPath;
        public string TemporaryDirectoryPath { get; private set; }
        public TemporaryStorageManager(string dependencyPath)
        {
            this.dependencyPath = dependencyPath;
            string tempId = Guid.NewGuid().ToString().Substring(0, 8);
            string tempDirectoryPath = Path.Combine(Path.GetTempPath(), tempId);
            DirectoryInfo temporaryDirectory = Directory.CreateDirectory(tempDirectoryPath);
            TemporaryDirectoryPath = temporaryDirectory.FullName;
        }

        public void CopyDependenciesToTemporaryDirectory()
        {

            foreach (FileInfo dependencyFile in new DirectoryInfo(dependencyPath).GetFiles())
            {
                dependencyFile.CopyTo(Path.Combine(TemporaryDirectoryPath, dependencyFile.Name));
            }
        }

        public string CreateFilePath(string fileName)
        {
            return Path.Combine(TemporaryDirectoryPath, fileName);
        }
    }
}