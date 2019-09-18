using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using ExternalProcessFunctions.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ExternalProcessFunctions.Tests
{
    [TestClass]
    public class RunExternalProcessUnitTest
    {
        public TestContext TestContext { get; set; }
        private string testId;
        private string testBaseDirectory;

        [TestInitialize]
        public void SetupTest()
        {
            testId = $"{TestContext.FullyQualifiedTestClassName}_{TestContext.TestName}_{Guid.NewGuid().ToString().Substring(0,8)}";
            testBaseDirectory = Path.Combine(Path.GetTempPath(), testId);
            Directory.CreateDirectory(testBaseDirectory);
        }

        [TestCleanup]
        public void CleanupTest()
        {
            Directory.Delete(testBaseDirectory, true);
        }

        [TestMethod]
        public async Task ReturnsOkResultWhenProcessExitCodeIs0()
        {
            Mock<ILogger> loggingMock = new Mock<ILogger>();
            ProcessRunResult processResult = CreateProcessRunResult(0);
            IExternalProcessManager externalProcessManagerMockObject = CreateExternalProcessManagerMock(processResult).Object;
            RunExternalProcess runExternalProcessFunction = CreateRunExternalProcess(externalProcessManagerMockObject);

            IActionResult result = await runExternalProcessFunction.Run(
                CreateHttpRequest(), loggingMock.Object,
                new ExecutionContext() {FunctionAppDirectory = testBaseDirectory});

            result.Should().BeOfType<OkObjectResult>();
            ((OkObjectResult) result).Value.Should().Be($"Success! Process output: {processResult.Output}");
        }

        [TestMethod]
        public async Task ReturnsErrorResultWhenProcessExitCodeIsNot0()
        {
            Mock<ILogger> loggingMock = new Mock<ILogger>();
            ProcessRunResult processResult = CreateProcessRunResult(new Random().Next(1, int.MaxValue));
            IExternalProcessManager externalProcessManagerMockObject = CreateExternalProcessManagerMock(processResult).Object;
            RunExternalProcess runExternalProcessFunction = CreateRunExternalProcess(externalProcessManagerMockObject);

            IActionResult result = await runExternalProcessFunction.Run(
                CreateHttpRequest(), loggingMock.Object,
                new ExecutionContext() {FunctionAppDirectory = testBaseDirectory});

            result.Should().BeOfType<ObjectResult>();
            ObjectResult objectResult = (ObjectResult) result;
            objectResult.Value.Should().Be($"Fail! Process error: {processResult.Error}");
            objectResult.StatusCode.Should().Be((int) HttpStatusCode.InternalServerError);
        }

        [TestMethod]
        public async Task CallsInitialiseOnExternalProcessManagerWithExitCodeQueryStringParameter()
        {
            Mock<ILogger> loggingMock = new Mock<ILogger>();
            ProcessRunResult processResult = CreateProcessRunResult(new Random().Next(1, int.MaxValue));
            Mock<IExternalProcessManager> externalProcessManagerMock = CreateExternalProcessManagerMock(processResult);
            IExternalProcessManager externalProcessManagerMockObject = externalProcessManagerMock.Object;
            string expectedExitCode = "testExitCode";
            RunExternalProcess runExternalProcessFunction = CreateRunExternalProcess(externalProcessManagerMockObject);

            IActionResult result = await runExternalProcessFunction.Run(
                CreateHttpRequest(expectedExitCode), loggingMock.Object,
                new ExecutionContext() { FunctionAppDirectory = testBaseDirectory });

            externalProcessManagerMock.Verify(mock => mock.Initialize(testBaseDirectory, expectedExitCode, true));
        }

        [TestMethod]
        public async Task CallsInitialiseOnExternalProcessManagerWithEmptyArgumentsWhenExitCodeQueryStringParameterDoesNotExist()
        {
            Mock<ILogger> loggingMock = new Mock<ILogger>();
            ProcessRunResult processResult = CreateProcessRunResult(new Random().Next(1, int.MaxValue));
            Mock<IExternalProcessManager> externalProcessManagerMock = CreateExternalProcessManagerMock(processResult);
            IExternalProcessManager externalProcessManagerMockObject = externalProcessManagerMock.Object;
            string expectedExitCode = "";
            RunExternalProcess runExternalProcessFunction = CreateRunExternalProcess(externalProcessManagerMockObject);

            IActionResult result = await runExternalProcessFunction.Run(
                CreateHttpRequest(), loggingMock.Object,
                new ExecutionContext() { FunctionAppDirectory = testBaseDirectory });

            externalProcessManagerMock.Verify(mock => mock.Initialize(testBaseDirectory, expectedExitCode, true));
        }

        private static DefaultHttpRequest CreateHttpRequest(string expectedExitCode = null)
        {
            DefaultHttpRequest request = new DefaultHttpRequest(new DefaultHttpContext());
            if (expectedExitCode != null)
            {
                request.QueryString = new QueryString($"?exitcode={expectedExitCode}");
            };
            return request; 
        }

        private static RunExternalProcess CreateRunExternalProcess(IExternalProcessManager externalProcessManagerMockObject)
        {
            return new RunExternalProcess(externalProcessManagerMockObject);
        }

        private static Mock<IExternalProcessManager> CreateExternalProcessManagerMock(ProcessRunResult processResult)
        {
            Mock<IExternalProcessManager> externalProcessManagerMock = new Mock<IExternalProcessManager>();
            externalProcessManagerMock.Setup(mock => mock.RunProcess()).Returns(processResult);
            return externalProcessManagerMock;
        }

        private static ProcessRunResult CreateProcessRunResult(int processExitCode)
        {
            return new ProcessRunResult("test output", "error output", processExitCode);
        }
    }
}
