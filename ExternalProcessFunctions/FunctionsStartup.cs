using ExternalProcessFunctions.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(ExternalProcessFunctions.Startup))]

namespace ExternalProcessFunctions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddScoped<ITemporaryStorageManager, TemporaryStorageManager>();
            builder.Services.AddTransient<IExternalProcessManager, ExternalProcessManager>();
            builder.Services.AddSingleton<IEnvironmentManager, EnvironmentManager>();   
        }
    }
}
