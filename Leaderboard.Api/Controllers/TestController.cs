using Leaderboard.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Leaderboard.Api;

[ApiController]
[Route("test")]
public class TestController : Controller
{
    private readonly ILeaderboardService _leaderboardService;

    public TestController(ILeaderboardService leaderboardService)
    {
        _leaderboardService = leaderboardService;
    }

    [HttpPost("create/data/demo")]
    public ActionResult<string> LoadDemoData()
    {
        var entities = GetDemoData();
        entities.ForEach(e => _leaderboardService.UpdateScore(e.CustomerId, e.Score));

        return Ok("Success");
    }

    [HttpPost("create/data/debug")]
    public ActionResult<string> LoadDebugData()
    {
        var entities = GetDebugData();
        entities.ForEach(e => _leaderboardService.UpdateScore(e.CustomerId, e.Score));

        return Ok("Success");
    }

    [HttpPost("create/data/load/{count}")]
    public ActionResult<string> LoadData([FromRoute] int count)
    {
        var entities = GeLoadData(count);
        entities.ForEach(e => _leaderboardService.UpdateScore(e.CustomerId, e.Score));

        return Ok("Success");
    }

    [HttpPost("create/data/multithreads")]
    public async Task<IActionResult> MultiThreads()
    {
        try
        {
            await RunMultiThreads();
            return Ok("Success");
        }
        catch (Exception ex) 
        {
            return BadRequest($"Failed: {ex.Message}");
        }
    }

    private List<Customer> GetDemoData()
    {
        return new List<Customer> 
        {
            new Customer { CustomerId = 6144320, Score = 93, Rank = 7 },
            new Customer { CustomerId = 8009471, Score = 93, Rank = 8 },
            new Customer { CustomerId = 11028481, Score = 93, Rank = 9 },
            new Customer { CustomerId = 38819, Score = 92, Rank = 10 },
            new Customer { CustomerId = 15514665, Score = 124, Rank = 1 },
            new Customer { CustomerId = 254814111, Score = 96, Rank = 5 },
            new Customer { CustomerId = 53274324, Score = 95, Rank = 6 },
            new Customer { CustomerId = 81546541, Score = 113, Rank = 2 },
            new Customer { CustomerId = 1745431, Score = 100, Rank = 3 },
            new Customer { CustomerId = 76786448, Score = 100, Rank = 4 },
        };
    }
    private List<Customer> GetDebugData()
    {
        return new List<Customer>
        {
            new Customer { CustomerId = 1001, Score = 1, Rank = 1 },
            new Customer { CustomerId = 1002, Score = 2, Rank = 2 },
            new Customer { CustomerId = 1003, Score = 3, Rank = 3 },
        };
    }

    private List<Customer> GeLoadData(int n)
    {
        var result = new List<Customer>();
        for (int i = 0; i < n; i++)
        {
            result.Add(new Customer { CustomerId = i + 1, Score = new Random().Next(-1000, 1000) });
        }

        return result;
    }

    private async Task RunMultiThreads()
    {
        // Arrange
        //var skipList = new ConcurrentSkipList();
        //var skipList = new CasSkipList();
        //var skipList = new LockFreeConcurrentSkipList();
        int threadCount = Environment.ProcessorCount * 2;
        //int threadCount = 5;
        //int[] recordCount = new[] { 1000, 2000, 4000, 8000 };
        int[] recordCount = new[] { 8 };
        var executionTimes = new Dictionary<int, double>();

        var writerTasks = new List<Task>();

        try
        {
            foreach (var count in recordCount)
            {
                var stopwatch = Stopwatch.StartNew();
                // Writer tasks
                for (int i = 0; i < threadCount; i++)
                {
                    writerTasks.Add(Task.Run(() =>
                    {
                        var random = new Random();
                        for (int j = 0; j < count; j++)
                        {
                            long customerId = random.Next(1, count);
                            decimal score = random.Next(-1000, 1000);
                            _leaderboardService.UpdateScore(customerId, score);
                        }
                    }));
                }

                // Wait for writers to complete
                await Task.WhenAll(writerTasks);

                stopwatch.Stop();
                executionTimes.Add(count, stopwatch.ElapsedMilliseconds);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }


        //Print("Update Score", threadCount, executionTimes);
    }
}
