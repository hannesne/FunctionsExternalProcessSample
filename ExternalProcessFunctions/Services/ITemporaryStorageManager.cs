namespace ExternalProcessFunctions.Services
{
    public interface ITemporaryStorageManager
    {
        string TemporaryDirectoryPath { get; }
        void CopyFolderContentsToTemporaryDirectory(string path);
        string CreateFilePath(string fileName);
    }
}