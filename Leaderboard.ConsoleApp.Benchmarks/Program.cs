using BenchmarkDotNet.Running;
using Leaderboard.Services;
using System.Diagnostics;

namespace Leaderboard.ConsoleApp.Benchmarks
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            BenchmarkRunner.Run<LeaderboardServiceBenchmarks>();
            await LargeScaleConcurrentInsertion();
        }
        private static async Task LargeScaleConcurrentInsertion()
        {
            Console.WriteLine("----------------Lock-Free Concurrent Skip List----------------");
            int[] threadCound = [100, 1000];
            int recordCount = 10000;
            int[] listCount = new int[threadCound.Length];
            int[] cacheCount = new int[threadCound.Length];
            long[] time = new long[threadCound.Length];
            int k = 0;
            foreach (var num in threadCound)
            {
                var skipList = new ConcurrentSkipList();
                Task[] tasks = new Task[num];
                var stop = new Stopwatch();
                stop.Start();
                for (int i = 0; i < num; i++)
                {
                    tasks[i] = Task.Run(() =>
                    {
                        for (int j = 0; j < recordCount; j++)
                        {
                            long customerId = new Random().Next(1, recordCount + 1);
                            decimal score = new Random().Next(-1000, 1001);
                            var r = skipList.AddOrUpdate(customerId, score);
                        }
                    });
                }
                await Task.WhenAll(tasks);
                stop.Stop();
                time[k] = stop.ElapsedMilliseconds;
                listCount[k] = skipList.Count;
                cacheCount[k++] = skipList.Cache.Count;
            }

            Console.WriteLine($"\nMethod{"".PadRight(6)} Count{"".PadRight(4)} ThreadCount{"".PadRight(3)} Mean{"".PadRight(4)}");
            for (var i = 0; i < threadCound.Length; i++)
            {
                Console.WriteLine($"{"AddOrUpdate".PadRight(8)} {recordCount}{"".PadRight(5)} {threadCound[i]}{"".PadRight(10)} {time[i]}{"ms".PadRight(2)} {(listCount[i], cacheCount[i])}");
            }

            Console.WriteLine();
        }
    }
}
