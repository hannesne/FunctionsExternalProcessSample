using System;
using System.Collections.Generic;
using System.Text;
using ExternalProcessFunctions.Services;
using FluentAssertions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ExternalProcessFunctions.Tests
{
    [TestClass]
    public class FunctionsStartupUnitTest
    {
        [TestMethod]
        public void CanResolveRegisteredServices()
        {
            Startup startup = new Startup();
            Mock<IFunctionsHostBuilder> builderMock = new Mock<IFunctionsHostBuilder>();
            IServiceCollection serviceCollection = new ServiceCollection();
            builderMock.SetupGet(mock => mock.Services).Returns(serviceCollection);
            IFunctionsHostBuilder builder = builderMock.Object;

            startup.Configure(builder);

            ServiceProvider provider = serviceCollection.BuildServiceProvider();
            object externalProcessManager = provider.GetService(typeof(IExternalProcessManager));
            externalProcessManager.Should().NotBeNull();
            externalProcessManager.Should().BeOfType<ExternalProcessManager>();
        }
    }
}
