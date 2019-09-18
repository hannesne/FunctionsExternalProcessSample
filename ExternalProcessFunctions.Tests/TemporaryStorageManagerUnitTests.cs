using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ExternalProcessFunctions.Services;
using FluentAssertions;
using FluentAssertions.Equivalency;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExternalProcessFunctions.Tests
{
    [TestClass]
    public class TemporaryStorageManagerUnitTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void SubsequentInstantiationsCreatesDifferentTemporaryStorageDirectoriesThatExists()
        {
            TemporaryStorageManager storageManager1 = new TemporaryStorageManager();
            TemporaryStorageManager storageManager2 = new TemporaryStorageManager();

            storageManager1.TemporaryDirectoryPath.Should().NotBe(storageManager2.TemporaryDirectoryPath);
            Directory.Exists(storageManager1.TemporaryDirectoryPath).Should().BeTrue();
            Directory.Exists(storageManager2.TemporaryDirectoryPath).Should().BeTrue();
        }

        [TestMethod]
        public void CopyFolderContentsToTemporaryDirectoryShouldCopyFolderContent()
        {
            TemporaryStorageManager storageManager = new TemporaryStorageManager();
            string sourcePath = CreateTemporaryFiles();
            string[] sourceFiles = GetFileNames(sourcePath);

            storageManager.CopyFolderContentsToTemporaryDirectory(sourcePath);

            string[] targetFiles = GetFileNames(storageManager.TemporaryDirectoryPath);
            targetFiles.Should().BeEquivalentTo(sourceFiles);

        }

        [TestMethod]
        public void CreateFilePathAppendsGivenFileNameToTemporaryDirectory()
        {
            TemporaryStorageManager storageManager = new TemporaryStorageManager();
            string fileName = "myfile.txt";
            string expectedFilePath = Path.Combine(storageManager.TemporaryDirectoryPath, fileName);
                
            string filePath = storageManager.CreateFilePath(fileName);

            filePath.Should().Be(expectedFilePath);
        }

        private static string[] GetFileNames(string sourcePath)
        {
            return Directory.GetFiles(sourcePath).Select(file => new FileInfo(file).Name).ToArray();
        }

        private string CreateTemporaryFiles()
        {
            string testId = $"{TestContext.FullyQualifiedTestClassName}_{TestContext.TestName}_{Guid.NewGuid().ToString().Substring(0, 8)}";
            string testBaseDirectory = Path.Combine(Path.GetTempPath(), testId);
            Directory.CreateDirectory(testBaseDirectory);
            File.AppendAllText(Path.Combine(testBaseDirectory,"test1.dat"),"test1");
            File.AppendAllText(Path.Combine(testBaseDirectory,"test2.txt"),"test2");
            return testBaseDirectory; 
        }
    }
}