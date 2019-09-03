using System.Diagnostics;
using System.IO;

namespace ExternalProcessFunctions.Services
{
    public class ExternalProcessManager
    {
        private readonly string executableSourcePath;
        private readonly string workingDirectoryPath;
        private readonly string arguments;
        private readonly bool redirectIo;

        public ExternalProcessManager(string arguments,
            TemporaryStorageManager temporaryStorageManager, bool redirectIO = true)
        {
            executableSourcePath = temporaryStorageManager.CreateFilePath("ExternalApp.exe");
            workingDirectoryPath = temporaryStorageManager.TemporaryDirectoryPath;
            this.arguments = arguments;
            this.redirectIo = redirectIO;
        }

        public ProcessRunResult RunProcess()
        {

            Process process = new Process
            {
                StartInfo =
                {
                    FileName = executableSourcePath,
                    WorkingDirectory = workingDirectoryPath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = redirectIo,
                    RedirectStandardError = redirectIo,
                    RedirectStandardInput = redirectIo
                }
            };
            process.Start();

            string processOutputStream = string.Empty;
            string processErrorStream = string.Empty;
            if (redirectIo)
            {
                processOutputStream = process.StandardOutput.ReadToEnd();
                processErrorStream = process.StandardError.ReadToEnd();
            }
            process.WaitForExit();

            return new ProcessRunResult(processOutputStream, processErrorStream, process.ExitCode);
        }
    }
}