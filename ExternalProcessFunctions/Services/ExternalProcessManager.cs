using System;
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
            if (!File.Exists(executableSourcePath))
                throw new ExternalProcessExecutableDoesNotExist(executableSourcePath);
            if (!Directory.Exists(workingDirectoryPath))
                throw new ExternalProcessWorkingDirectoryDoesNotExist(workingDirectoryPath);
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

    public class ExternalProcessWorkingDirectoryDoesNotExist : Exception
    {
        public ExternalProcessWorkingDirectoryDoesNotExist(string path) : base($"The working directory could not be found at {path}.")
        {
            
        }
    }

    public class ExternalProcessExecutableDoesNotExist : Exception
    {
        public ExternalProcessExecutableDoesNotExist(string path) : base($"The external executable could not be found at {path}.")
        {
            
        }
    }
}