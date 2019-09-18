using System.Net;
using System.Threading.Tasks;
using ExternalProcessFunctions.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace ExternalProcessFunctions
{
    public class RunExternalProcess
    {
        private readonly IExternalProcessManager processManager;

        public RunExternalProcess(IExternalProcessManager processManager)
        {
            this.processManager = processManager;
        }

        [FunctionName("RunExternalProcess")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            StringValues expectedExitCodeValues = req.Query["exitcode"];
            
            processManager.Initialize(context.FunctionAppDirectory, expectedExitCodeValues.Count == 0 ? "" : expectedExitCodeValues.ToString());

            ProcessRunResult result = processManager.RunProcess();
            return result.ProcessExitCode == 0
                ? new OkObjectResult($"Success! Process output: {result.Output}")
                : new ObjectResult($"Fail! Process error: {result.Error}"){StatusCode = (int?) HttpStatusCode.InternalServerError};
        }
    }
}
