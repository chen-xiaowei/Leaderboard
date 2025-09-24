namespace Leaderboard.Services;

public class LeaderboardService : ILeaderboardService
{
    private readonly ConcurrentSkipList _skipList = new ConcurrentSkipList();

    public decimal UpdateScore(long customerId, decimal score)
    {
        return _skipList.AddOrUpdate(customerId, score);
    }

    public IEnumerable<Customer> GetCustomersByRank(int start, int end)
    {
        if (start < 1) start = 1;
        if (end < start) end = start;

        return _skipList.GetRange(start, end);
    }

    public IEnumerable<Customer> GetCustomersByCustomerId(long customerId, int high, int low)
    {
        return _skipList.GetCustomersById(customerId, high, low);
    }
}