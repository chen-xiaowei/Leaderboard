namespace Leaderboard.Services;

public interface ILeaderboardService
{
    decimal UpdateScore(long customerId, decimal score);
    IEnumerable<Customer> GetCustomersByRank(int start, int end);
    IEnumerable<Customer> GetCustomersByCustomerId(long customerId, int high, int low);
}
