using Dotmim.Sync;
using Dotmim.Sync.Enumerations;
using Dotmim.Sync.SqlServer;
using Lap.Agent.Config;
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
        var srcProvider = new SqlSyncProvider(Source);
        var tgtProvider = new SqlSyncProvider(Target);

        var setup = new SyncSetup(Options.Targets);
        
        var agent=new SyncAgent(tgtProvider,srcProvider);
        agent.Options.BatchSize = Options.Batch;

        var progress = new ConsoleProgress();

        
        var res=await agent.SynchronizeAsync(setup,SyncType.Reinitialize,progress ).ConfigureAwait(false);
        
        return res;
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