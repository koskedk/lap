using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Lap.Agent.Tests;

[SetUpFixture]
public class TestInitializer
{
    public static IConfiguration? Configuration;
    
    [OneTimeSetUp]
    public void Init()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .CreateLogger();

        SetupDependencyInjection();
    }

    private void SetupDependencyInjection()
    {
        var config = Configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Test.json", optional: false, reloadOnChange: true)
            .Build();
        var services = new ServiceCollection();
    }
}