using System.Diagnostics;
using System.IO;

namespace ExternalProcessFunctions.Services
{
    public class ExternalProcessManager
    {
        private readonly string executableSourcePath;
        private readonly string workingDirectoryPath;
        private readonly string arguments;
        private readonly bool readOutput;

        public ExternalProcessManager(string arguments,
            TemporaryStorageManager temporaryStorageManager, bool readOutput = true)
        {
            executableSourcePath = temporaryStorageManager.CreateFilePath("ExternalApp.exe");
            workingDirectoryPath = temporaryStorageManager.TemporaryDirectoryPath;
            this.arguments = arguments;
            this.readOutput = readOutput;
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
                    RedirectStandardOutput = readOutput,
                    RedirectStandardError = readOutput,
                    RedirectStandardInput = readOutput
                }
            };
            process.Start();

            string processOutputStream = string.Empty;
            string processErrorStream = string.Empty;
            if (readOutput)
            {
                processOutputStream = process.StandardOutput.ReadToEnd();
                processErrorStream = process.StandardError.ReadToEnd();
            }
            process.WaitForExit();

            return new ProcessRunResult(processOutputStream, processErrorStream, process.ExitCode);
        }
    }
}