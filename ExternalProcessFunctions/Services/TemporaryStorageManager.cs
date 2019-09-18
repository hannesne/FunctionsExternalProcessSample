using System;
using System.IO;

namespace ExternalProcessFunctions.Services
{
    public class TemporaryStorageManager : ITemporaryStorageManager
    {
        public string TemporaryDirectoryPath { get; private set; }
        public TemporaryStorageManager()
        {
            string tempId = Guid.NewGuid().ToString().Substring(0, 8);
            string tempDirectoryPath = Path.Combine(Path.GetTempPath(), tempId);
            DirectoryInfo temporaryDirectory = Directory.CreateDirectory(tempDirectoryPath);
            TemporaryDirectoryPath = temporaryDirectory.FullName;
        }

        public void CopyFolderContentsToTemporaryDirectory(string path)
        {

            foreach (FileInfo dependencyFile in new DirectoryInfo(path).GetFiles())
            {
                dependencyFile.CopyTo(Path.Combine(TemporaryDirectoryPath, dependencyFile.Name),true);
            }
        }

        public string CreateFilePath(string fileName)
        {
            return Path.Combine(TemporaryDirectoryPath, fileName);
        }
    }
}