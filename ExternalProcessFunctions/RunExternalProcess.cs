using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using ExternalProcessFunctions.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.DotNet.PlatformAbstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace ExternalProcessFunctions
{
    public static class RunExternalProcess
    {
        [FunctionName("RunExternalProcess")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            TemporaryStorageManager storageManager = BuildStorageManager(context);
            storageManager.CopyDependenciesToTemporaryDirectory();

            StringValues expectedExitCode = req.Query["exitcode"];
            ExternalProcessManager processManager = new ExternalProcessManager(expectedExitCode, storageManager);
            ProcessRunResult result = processManager.RunProcess();
            return result.ProcessExitCode == 0
                ? new OkObjectResult($"Success! Process output: {result.Output}")
                : new ObjectResult($"Fail! Process error: {result.Error}"){StatusCode = (int?) HttpStatusCode.InternalServerError};
        }

        private static TemporaryStorageManager BuildStorageManager(ExecutionContext context)
        {
            string architecture = Environment.Is64BitOperatingSystem ? "x64" : "Win32";
            string dependencyPath = Path.Combine(context.FunctionAppDirectory, "DependencyExe", architecture);
            TemporaryStorageManager storageManager = new TemporaryStorageManager(dependencyPath);
            return storageManager;
        }
    }
}
