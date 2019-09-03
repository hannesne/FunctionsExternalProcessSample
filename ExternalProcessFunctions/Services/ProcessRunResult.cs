namespace ExternalProcessFunctions.Services
{
    public class ProcessRunResult
    {
        public ProcessRunResult(string output, string error, int processExitCode)
        {
            Output = output;
            Error = error;
            ProcessExitCode = processExitCode;
        }
        public string Output { get; set; }
        public string Error { get; set; }
        public int ProcessExitCode { get; }
    }
}