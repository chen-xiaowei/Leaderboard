using Leaderboard.Services;

namespace Leaderboard.Tests;

public class ConcurrentSkipListTests
{
    [Fact]
    public async Task HighConcurrencyCrossTesting_Expect_Success()
    {
        // Arrange
        var skipList = new ConcurrentSkipList();

        int customerCount = 3000;
        int threadCount = 30;  // 30 threads
        int recordCount = 100000;
        
        // Act
        var updateTasks = new List<Task>();
        var queryTasks = new List<Task>();

        // Update tasks
        for (int i = 0; i < threadCount; i++)
        {
            updateTasks.Add(Task.Run(() =>
            {
                var random = new Random();
                for (int j = 0; j < recordCount; j++)
                {
                    long customerId = random.Next(1, customerCount + 1);
                    decimal score = random.Next(-1000, 1001);

                    var result = skipList.AddOrUpdate(customerId, score);

                    // Verify the returned score
                    skipList.Cache.TryGetValue(customerId, out decimal expectedScore);

                    Assert.Equal(expectedScore, result);
                }
            }));
        }

        // Query tasks
        for (int i = 0; i < threadCount / 2; i++)
        {
            queryTasks.Add(Task.Run(() =>
            {
                var random = new Random();

                // Test GetCustomersById
                long customerId = random.Next(1, customerCount + 1);
                var result = skipList.GetCustomersById(customerId);

                if (result.Count > 0)
                {
                    skipList.Cache.TryGetValue(customerId, out decimal expectedScore);
                    Assert.Equal(expectedScore, result.FirstOrDefault()!.Score);
                }

                // Test GetRange 
                if (random.NextDouble() < 0.1)
                {
                    int start = random.Next(0, Math.Max(1, skipList.Count - 10));
                    int end = start + random.Next(1, 10);
                    var range = skipList.GetRange(start, end);

                    // Verify whether the range is ordered
                    for (int j = 1; j < range.Count; j++)
                    {
                        Assert.True(range[j - 1].Score >= range[j].Score);
                    }
                }
            }));
        }

        await Task.WhenAll(updateTasks);
        await Task.WhenAll(queryTasks);

        // Data consistency checking
        foreach (var expected in skipList.Cache)
        {
            var result = skipList.GetCustomersById(expected.Key);
            var actual = result.FirstOrDefault();
            Assert.NotNull(actual);
            Assert.Equal(expected.Value, actual.Score);

            // Verify rank
            if (actual.Rank > 2)
            {
                var higherScore = skipList.GetRange(actual.Rank - 1, actual.Rank - 1).FirstOrDefault();
                if (higherScore != null)
                {
                    Assert.True(higherScore.Score >= actual.Score);
                }
            }
        }

        // Final checking
        Assert.True(skipList.Count == skipList.Cache.Count);
        Assert.True(skipList.Level >= 1 && skipList.Level <= 64);
    }
}