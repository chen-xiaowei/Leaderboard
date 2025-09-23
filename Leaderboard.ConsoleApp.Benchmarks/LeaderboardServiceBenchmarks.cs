using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Mathematics;
using BenchmarkDotNet.Order;
using Leaderboard.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leaderboard.ConsoleApp.Benchmarks;

[SimpleJob(RunStrategy.Throughput)]
[MemoryDiagnoser(false)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn(NumeralSystem.Arabic)]
public class LeaderboardServiceBenchmarks
{
    private readonly ILeaderboardService _service = new LeaderboardService();

    [Params(1000)]
    public int RecordCount { get; set; }

    [Params(5, 10, 25)]
    public int ThreadCount { get; set; }

    [Benchmark(Baseline = true)]
    public void UpdateScore()
    {
        var skipList = new ConcurrentSkipList();
        var tasks = new Task[ThreadCount];

        for (int i = 0; i < ThreadCount; i++)
        {
            tasks[i] = Task.Run(() =>
            {
                for (int j = 0; j < RecordCount; j++)
                {
                    long customerId = new Random().Next(1, 10000);
                    decimal score = new Random().Next(-1000, 1001);
                    _service.UpdateScore(customerId, score);
                }
            });
        }
        Task.WaitAll(tasks);
    }

    [Benchmark]
    public void GetCustomersByRank()
    {
        var tasks = new Task[ThreadCount];

        for (int i = 0; i < ThreadCount; i++)
        {
            tasks[i] = Task.Run(() =>
            {
                _service.GetCustomersByRank(10, 20);
            });
        }
        Task.WaitAll(tasks);
    }

    [Benchmark]
    public void GetCustomersByCustomerId()
    {
        var tasks = new Task[ThreadCount];

        for (int i = 0; i < ThreadCount; i++)
        {
            tasks[i] = Task.Run(() =>
            {
                long customerId = new Random().Next(1000, 6000);
                _service.GetCustomersByCustomerId(customerId, 10, 20);
            });
        }
        Task.WaitAll(tasks);
    }
}
