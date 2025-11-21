using Dapper;
using Microsoft.Data.SqlClient;
using Serilog;

namespace Lap.Agent.Tests;

[TestFixture]
public class TransferJobTests
{
    private TransferJob _transferJob;
    
    [SetUp]
    public void Setup()
    {
        Assert.That(TestInitializer.Configuration,Is.Not.Null);
        _transferJob = new TransferJob(TestInitializer.Configuration);
    }

    [Test]
    public async Task ShouldRun()
    {
        var res= await _transferJob.RunAsync();
        Log.Information(res.ToString());
        Assert.That(_transferJob.Options.Targets.Length,Is.GreaterThan(0));
        
        foreach (var target in _transferJob.Options.Targets)
            Assert.That(CheckCount(target), Is.GreaterThan(0));
        
    }

    private int CheckCount(string target)
    {
        var sql =$"select count (0) from {target}";

        using var cn=new SqlConnection(_transferJob.Target);
        return cn.ExecuteScalar<int>(sql);
    }
}