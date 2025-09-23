using Leaderboard.Services;
using Microsoft.AspNetCore.Mvc;

namespace Leaderboard.Api;

[ApiController]
[Route("leaderboard")]
public class LeaderboardController : ControllerBase
{
    private readonly ILeaderboardService _leaderboardService;

    public LeaderboardController(ILeaderboardService leaderboardService)
    {
        _leaderboardService = leaderboardService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCustomersByRank([FromQuery] int start, [FromQuery] int end)
    {
        if (start < 1) return BadRequest("Start must be greater than 0.");
        if (end < start) return BadRequest("End must be greater than or equal to start.");

        var result = await Task.Run(() => { return _leaderboardService.GetCustomersByRank(start, end); });
        return Ok(result);
    }
    
    [HttpGet("{customerId}")]
    public async Task<IActionResult> GetCustomersByCustomerId([FromRoute] long customerId,
        [FromQuery] int high = 0, [FromQuery] int low = 0)
    {
        if (high < 0) return BadRequest("High must be non-negative.");
        if (low < 0) return BadRequest("Low must be non-negative.");

        var result = await Task.Run(() => { return _leaderboardService.GetCustomersByCustomerId(customerId, high, low); });
        return Ok(result);
    }
}