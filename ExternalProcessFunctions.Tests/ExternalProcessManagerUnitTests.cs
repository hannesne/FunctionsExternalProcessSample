using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using ExternalProcessFunctions.Services;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ExternalProcessFunctions.Tests
{
    [TestClass]
    public class ExternalProcessManagerUnitTests
    {
        private static readonly string baseDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static readonly string expectedExecutablePath = Path.Combine(baseDirectory, "DependencyExe", Environment.Is64BitProcess ? "x64" : "Win32");

        [DataTestMethod]
        [DataRow(true, "x64")]
        [DataRow(false, "Win32")]
        public void InitializeCopiesDependencyContentsToTemporaryDirectoryForDifferentArchitectures(bool is64Bit, string architecture)
        {
            Mock<ITemporaryStorageManager> storageManagerMock = new Mock<ITemporaryStorageManager>();

            ExternalProcessManager processManager = CreateExternalProcessManager(storageManagerMock.Object, is64Bit);

            processManager.Initialize(baseDirectory, "");

            string expectedPath = Path.Combine(baseDirectory, "DependencyExe", architecture);
            storageManagerMock.Verify(mock => mock.CopyFolderContentsToTemporaryDirectory(expectedPath));
        }

        [TestMethod]
        public void InitializeThrowsExceptionIfDependencyPathDoesNotExist()
        {
            Mock<ITemporaryStorageManager> storageManagerMock = new Mock<ITemporaryStorageManager>();
            ExternalProcessManager processManager = CreateExternalProcessManager(storageManagerMock.Object);

            string invalidBaseDirectory = Path.Combine(baseDirectory, "DoesNotExist");

            Assert.ThrowsException<DependencyPathDoesNotExistException>(() => processManager.Initialize(invalidBaseDirectory, ""));
        }

        [TestMethod]
        public void RunProcessThrowsExceptionIfExecutableDoesNotExist()
        {
            Mock<ITemporaryStorageManager> storageManagerMock = new Mock<ITemporaryStorageManager>();
            storageManagerMock.Setup(mock => mock.CreateFilePath("ExternalApp.exe")).Returns("DoesNotExistsPath.exe");
            ExternalProcessManager processManager = CreateExternalProcessManager(storageManagerMock.Object);
            processManager.Initialize(baseDirectory, "");

            Assert.ThrowsException<ExternalProcessExecutableDoesNotExist>(() => processManager.RunProcess());
        }

        [TestMethod]
        public void RunProcessThrowsExceptionIfWorkingDirectoryDoesNotExist()
        {
            Mock<ITemporaryStorageManager> storageManagerMock = new Mock<ITemporaryStorageManager>();
            string executableName = "ExternalApp.exe";
            storageManagerMock.Setup(mock => mock.CreateFilePath(executableName)).Returns(Path.Combine(expectedExecutablePath, executableName));
            //storageManagerMock.SetupGet(mock => mock.TemporaryDirectoryPath).Returns(expectedExecutablePath);
            storageManagerMock.SetupGet(mock => mock.TemporaryDirectoryPath).Returns(Path.Combine(expectedExecutablePath, "DoesNotExist"));
            ExternalProcessManager processManager = CreateExternalProcessManager(storageManagerMock.Object);
            processManager.Initialize(baseDirectory, "");

            Assert.ThrowsException<ExternalProcessWorkingDirectoryDoesNotExist>(() => processManager.RunProcess());
        }

        [TestMethod]
        public void RunProcessReturnsProcessResultFromExecutable()
        {
            Mock<ITemporaryStorageManager> storageManagerMock = new Mock<ITemporaryStorageManager>();
            string executableName = "ExternalApp.exe";
            storageManagerMock.Setup(mock => mock.CreateFilePath(executableName)).Returns(Path.Combine(expectedExecutablePath, executableName));
            storageManagerMock.SetupGet(mock => mock.TemporaryDirectoryPath).Returns(expectedExecutablePath);
            ExternalProcessManager processManager = CreateExternalProcessManager(storageManagerMock.Object);
            processManager.Initialize(baseDirectory, "5");

            ProcessRunResult result = processManager.RunProcess();

            result.ProcessExitCode.Should().Be(5);
            result.Output.Should().Be($"Hello World!{Environment.NewLine}");
            result.Error.Should().Be($"Sad Trombone...{Environment.NewLine}");
        }

        private static ExternalProcessManager CreateExternalProcessManager(ITemporaryStorageManager storageManager)
        {
            return CreateExternalProcessManager(storageManager, Environment.Is64BitOperatingSystem);
        }

        private static ExternalProcessManager CreateExternalProcessManager(ITemporaryStorageManager storageManager,
            bool is64Bit)
        {
            Mock<IEnvironmentManager> environmentManagerMock = new Mock<IEnvironmentManager>();
            environmentManagerMock.SetupGet(mock => mock.Is64Bit).Returns(is64Bit);
            IEnvironmentManager environmentManager = environmentManagerMock.Object;

            return new ExternalProcessManager(storageManager, environmentManager);
        }
    }
}
