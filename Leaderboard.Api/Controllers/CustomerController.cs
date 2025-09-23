using Leaderboard.Services;
using Microsoft.AspNetCore.Mvc;

namespace Leaderboard.Api;

[ApiController]
[Route("customer")]
public class CustomerController : ControllerBase
{
    private readonly ILeaderboardService _leaderboardService;

    public CustomerController(ILeaderboardService leaderboardService)
    {
        _leaderboardService = leaderboardService;
    }
    
    [HttpPost("{customerId}/score/{score}")]
    public async Task<IActionResult> UpdateScore([FromRoute] long customerId, [FromRoute] decimal score)
    {
        if (score < -1000 || score > 1000)
            return BadRequest("Score must be between -1000 and 1000.");

        var result = await Task.Run(() => { return _leaderboardService.UpdateScore(customerId, score); });
        return Ok(result);
    }
}
