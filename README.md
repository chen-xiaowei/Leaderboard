# Leaderboard
## LeaderboardService: Benchmark Test Result
RunStrategy=Throughput

| Method                   | RecordCount | ThreadCount | Mean           | Error         | StdDev        | Ratio | RatioSD | Rank | Allocated  | Alloc Ratio |
|------------------------- |------------ |------------ |---------------:|--------------:|--------------:|------:|--------:|-----:|-----------:|------------:|
| GetCustomersByRank       | 1000        | 5           |       1.994 us |     0.0398 us |     0.0686 us | 0.000 |    0.00 |    1 |      994 B |       0.000 |
| GetCustomersByCustomerId | 1000        | 5           |       2.430 us |     0.0386 us |     0.0361 us | 0.000 |    0.00 |    2 |     1360 B |       0.000 |
| UpdateScore              | 1000        | 5           |  61,219.103 us | 1,215.1591 us | 2,096.0900 us | 1.001 |    0.05 |    3 |  8375378 B |       1.000 |
|                          |             |             |                |               |               |       |         |      |            |             |
| GetCustomersByRank       | 1000        | 10          |       3.449 us |     0.0390 us |     0.0365 us | 0.000 |    0.00 |    1 |     1866 B |       0.000 |
| GetCustomersByCustomerId | 1000        | 10          |       3.877 us |     0.0774 us |     0.0979 us | 0.000 |    0.00 |    2 |     2599 B |       0.000 |
| UpdateScore              | 1000        | 10          | 137,417.410 us | 2,727.0531 us | 3,732.8214 us | 1.001 |    0.04 |    3 | 16750786 B |       1.000 |
|                          |             |             |                |               |               |       |         |      |            |             |
| GetCustomersByCustomerId | 1000        | 25          |       9.289 us |     0.1822 us |     0.2238 us | 0.000 |    0.00 |    1 |     6311 B |       0.000 |
| GetCustomersByRank       | 1000        | 25          |       9.775 us |     0.1558 us |     0.1458 us | 0.000 |    0.00 |    1 |     4486 B |       0.000 |
| UpdateScore              | 1000        | 25          | 308,547.437 us | 6,065.4112 us | 8,097.1504 us | 1.001 |    0.04 |    2 | 41861704 B |       1.000 |

## ConcurrentSkipList: Large-Scale Concurrent Insertion Results in Console Application
----------------Lock-Free Concurrent Skip List----------------

| Method         | RecordCount  | ThreadCount | Mean    |
|--------------- |------------  |------------ |--------:|
| AddOrUpdate    | 10000        | 100         |  1532ms |
| AddOrUpdate    | 10000        | 1000        | 13215ms |