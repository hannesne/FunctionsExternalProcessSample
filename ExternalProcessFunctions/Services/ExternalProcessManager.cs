using System;
using System.Diagnostics;
using System.IO;

namespace ExternalProcessFunctions.Services
{
    public class ExternalProcessManager : IExternalProcessManager
    {
        private readonly string executableSourcePath;
        private readonly string workingDirectoryPath;
        private string arguments;
        private bool redirectIo;
        private readonly ITemporaryStorageManager temporaryStorageManager;
        private readonly IEnvironmentManager environmentManager;

        public ExternalProcessManager(ITemporaryStorageManager temporaryStorageManager,
            IEnvironmentManager environmentManager)
        {
            executableSourcePath = temporaryStorageManager.CreateFilePath("ExternalApp.exe");
            workingDirectoryPath = temporaryStorageManager.TemporaryDirectoryPath;
            this.temporaryStorageManager = temporaryStorageManager;
            this.environmentManager = environmentManager;
        }

        public void Initialize(string baseDirectoryPath, string arguments, bool redirectIO = true)
        {
            string architecture = environmentManager.Is64Bit ? "x64" : "Win32";
            string dependencyPath = Path.Combine(baseDirectoryPath, "DependencyExe", architecture);

            if (!Directory.Exists(dependencyPath))
                throw new DependencyPathDoesNotExistException(dependencyPath);
            
            temporaryStorageManager.CopyFolderContentsToTemporaryDirectory(dependencyPath);

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

    public class DependencyPathDoesNotExistException : Exception
    {
        public DependencyPathDoesNotExistException(string path) : base ($"The dependency path could not be found at {path}.")
        {
            
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