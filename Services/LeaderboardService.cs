namespace Leaderboard.Api;

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
        var result = new List<Customer>();

        var customerInfo = _skipList.GetScoreAndRank(customerId);
        if (!customerInfo.HasValue) return result;

        int rank = customerInfo.Value.Rank;

        if(high > 0)
        {
            int highStart = Math.Max(1, rank - high);
            int highEnd = rank - 1;
            if (highEnd >= highStart)
            {
                result.AddRange(_skipList.GetRange(highStart, highEnd));
            }
        }
        
        result.Add(new Customer
        {
            CustomerId = customerId,
            Score = customerInfo.Value.Score,
            Rank = rank
        });
        
        if(low > 0)
        {
            int lowStart = rank + 1;
            int lowEnd = rank + low;
            if (lowStart <= lowEnd)
            {
                result.AddRange(_skipList.GetRange(lowStart, lowEnd));
            }
        }

        return result;
    }
}