using Dotmim.Sync;
using Dotmim.Sync.Enumerations;
using Dotmim.Sync.SqlServer;
using Lap.Agent.Config;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Lap.Agent;

public class TransferJob
{
    public string Source { get; }
    public string Target { get; }
    public TransfersOptions Options { get; }
    
    public TransferJob(IConfiguration configuration)
    {
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        if (configuration.GetConnectionString("LiveConnectionSource") == null)
            throw new Exception("Missing Source configuration file");

        if (configuration.GetConnectionString("LiveConnectionTarget") == null)
            throw new Exception("Missing Source configuration file");

        Source = configuration.GetConnectionString("LiveConnectionSource");
        Target = configuration.GetConnectionString("LiveConnectionTarget");
        Options = configuration.GetSection(TransfersOptions.Transfers).Get<TransfersOptions>();
    }

    public async Task<SyncResult> RunAsync()
    {
        await VerifyConnections();
        
        var srcProvider = new SqlSyncProvider(Source);
        var tgtProvider = new SqlSyncProvider(Target);

        var setup = new SyncSetup(Options.Targets);
        
        foreach (var target in Options.Targets)
            setup.Tables[$"{target}"].SyncDirection = SyncDirection.DownloadOnly;
        
        var agent=new SyncAgent(tgtProvider,srcProvider);
        agent.Options.BatchSize = Options.Batch * 1024 * 1024;
     
        agent.Options.DisableConstraintsOnApplyChanges = true;

        var progress = new ConsoleProgress();
        
        SyncType syncType = SyncType.Normal; 
        if (Options.Mode.HasValue)
        {
            syncType = (SyncType)Options.Mode.Value;
            
            if (!Enum.IsDefined(typeof(SyncType), syncType))
            {
                Log.Warning($"Invalid integer value {Options.Mode.Value} for SynchronizationType. Defaulting to Normal.");
                syncType = SyncType.Normal;
            }
        }
        
        
        var res=await agent.SynchronizeAsync(setup,syncType,progress ).ConfigureAwait(false);
        
        return res;
    }

    private async Task VerifyConnections()
    {
        Log.Information("Verifying connections");
        try
        {
            await using var cn=new SqlConnection(Source);
            await cn.OpenAsync();
            await cn.CloseAsync();
            Log.Information("Verifying connections [SOURCE] OK ");
        }
        catch (Exception e)
        {
            Log.Error(e, $"SOURCE Error {Source}");
            throw;
        }
        
        try
        {
            await using var cn=new SqlConnection(Target);
            await cn.OpenAsync();
            await cn.CloseAsync();
            Log.Information("Verifying connections [TARGET] OK ");
        }
        catch (Exception e)
        {
            Log.Error(e, $"TARGET Error {Target}");
            throw;
        }
    }

    public class ConsoleProgress : IProgress<ProgressArgs>
    {
        public void Report(ProgressArgs s)
        {
            //Console.Write($"{s.ProgressPercentage:p}:  \t[{s.Source[..Math.Min(4, s.Source.Length)]}] {s.TypeName}: {s.Message}");
            Log.Information($"{s.ProgressPercentage:p}:  \t[{s.Source[..Math.Min(4, s.Source.Length)]}] {s.TypeName}: {s.Message}");
        }
    }

}