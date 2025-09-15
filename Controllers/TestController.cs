using Microsoft.AspNetCore.Mvc;

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
}
