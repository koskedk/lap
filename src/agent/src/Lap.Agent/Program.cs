using Dotmim.Sync.SqlServer;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Lap.Agent;

class Program
{
    public static IConfiguration configuration { get; set; }
    
    static async Task Main(string[] args)
    {
        
        configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
        
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();
        
        Log.Information("Lap.Agent Starting Up");
        
        try
        {
            Log.Information("Lap.Agent Starting Up");
            await Syncdbs().ConfigureAwait(false);
            Log.Warning("A sample warning message.");
        }
        catch (System.Exception ex)
        {
            Log.Fatal(ex, "Lap.Agent terminated unexpectedly");
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }



    private static async Task Syncdbs()
    {
        Log.Information("Starting Syncdbs...");
        var job = new TransferJob(configuration);
        var res = await job.RunAsync();
        Log.Information(res.ToString());
        Log.Information("Syncdbs finished");
    }
}