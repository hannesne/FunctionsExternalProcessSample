namespace ExternalProcessFunctions.Services
{
    public interface IExternalProcessManager
    {
        void Initialize(string baseDirectoryPath, string arguments, bool redirectIO = true);
        ProcessRunResult RunProcess();
    }
}