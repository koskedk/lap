namespace Lap.Agent.Config;

public class TransfersOptions
{
    public const string Transfers = "Transfers";
    public string[] Targets { get; set; } = [];
    public int Batch { get; set; }
}