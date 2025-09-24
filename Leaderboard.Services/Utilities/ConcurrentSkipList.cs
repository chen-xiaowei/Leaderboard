using System.Collections.Concurrent;

namespace Leaderboard.Services;

public class ConcurrentSkipList
{
    private readonly int _maxLevel = 64;
    private readonly double _probability = 0.25;
    private readonly Random _random;
    private readonly AtomicLock _atomicLock;
    private readonly ConcurrentDictionary<long, decimal> _cache;
    private Node _head;
    private volatile int _count;
    private volatile int _level;

    public ConcurrentSkipList()
    {
        _random = new Random();
        _atomicLock = new AtomicLock();
        _cache = new ConcurrentDictionary<long, decimal>();
        _head = new Node(0, 0, _maxLevel);
        Interlocked.Exchange(ref _level, 1);
    }

    public int Count => _count;
    public int Level => _level;
    public ConcurrentDictionary<long, decimal> Cache => _cache;

    private int GetRandomLevel()
    {
        int level = 1;
        while (_random.NextDouble() < _probability && level < _maxLevel)
            level++;
        return level;
    }

    public decimal AddOrUpdate(long customerId, decimal score)
    {
        var spinWait = new SpinWait();

        while (true)
        {
            if (_atomicLock.TryLock())
            {
                try
                {
                    (bool IsSuccess, decimal NewScore) = TryAddOrUpdate(customerId, score);
                    if (IsSuccess)
                        return NewScore;
                }
                finally
                {
                    _atomicLock.ExitLock();
                }
            }
            spinWait.SpinOnce();

            if (spinWait.Count % 200 == 0)
            {
                Thread.Sleep(0);
            }
        }
    }

    private (bool, decimal) TryAddOrUpdate(long customerId, decimal score)
    {
        bool found = _cache.TryGetValue(customerId, out decimal oldScore);
        decimal newScore = oldScore + score;
        if (newScore == oldScore) return (true, newScore);

        // Remove the existing node before adding
        if (found && !TryRemove(customerId, oldScore))
        {
            return (false, newScore);
        }

        // Find the update node and calculate ranking
        Node[] update = new Node[_maxLevel];
        int[] rank = new int[_maxLevel];
        Node current = _head;

        for (var i = _level - 1; i >= 0; i--)
        {
            rank[i] = i == _level - 1 ? 0 : rank[i + 1];
            while (current.Next[i] != null &&
                  (current.Next[i].Score > newScore ||
                  (current.Next[i].Score == newScore && current.Next[i].CustomerId < customerId)))
            {
                rank[i] += current.Span[i];
                current = current.Next[i];
            }
            update[i] = current;
        }

        int newLevel = GetRandomLevel();
        if (newLevel > _level)
        {
            newLevel = Interlocked.Increment(ref _level);
            update[newLevel - 1] = _head;
        }

        // Create new node
        Node newNode = new Node(customerId, newScore, newLevel);

        for (int i = 0; i < newLevel; i++)
        {
            newNode.Next[i] = update[i].Next[i];
            update[i].Next[i] = newNode;

            // Calculate the span and update the ranking
            if (newNode.Next[i] == null)
            {
                newNode.Span[i] = 0;
            }
            else if (update[i].Next[i] != null)
            {
                newNode.Span[i] = update[i].Span[i] - (rank[0] - rank[i]);
            }
            else
            {
                newNode.Span[i] = 1;
            }
            update[i].Span[i] = rank[0] - rank[i] + 1;
        }

        // Adjust the upper span
        for (int i = newLevel; i < _level; i++)
        {
            if (update[i] != null && update[i].Next[i] != null)
                update[i].Span[i]++;
        }

        _cache[customerId] = newScore;
        Interlocked.Increment(ref _count);

        return (true, newNode.Score);
    }

    public List<Customer> GetCustomersById(long customerId, int high, int low)
    {
        if (_count == 0 || !_cache.TryGetValue(customerId, out decimal oldScore)) return [];

        Node current = _head;
        int rank = 0;

        for (int i = _level - 1; i >= 0; i--)
        {
            while (current.Next[i] != null &&
                  (current.Next[i].Score > oldScore ||
                   (current.Next[i].Score == oldScore && current.Next[i].CustomerId < customerId)))
            {
                rank += current.Span[i];
                current = current.Next[i];
            }
        }

        Node target = current.Next[0];
        if (current.Next[0] == null ||
               current.Next[0].CustomerId != customerId)
        {
            return [];
        }
        rank++;

        int capacity = 1 + Math.Max(0, Math.Min(high, rank - 1)) + Math.Min(low, _count - rank);
        var result = new List<Customer>(capacity);

        if (high > 0)
        {
            int highStart = Math.Max(1, rank - high);
            int highEnd = rank - 1;
            if (highEnd >= highStart)
            {
                var highNodes = GetRange(highStart, highEnd);
                if (highNodes.Count > 0)
                {
                    result.AddRange(highNodes);
                }
            }
        }

        result.Add(new Customer
        {
            CustomerId = target.CustomerId,
            Score = target.Score,
            Rank = rank
        });

        if (low > 0)
        {
            int lowStart = rank + 1;
            int lowEnd = Math.Min(rank + low, _count);
            Node lowNode = target.Next[0];
            while (lowNode != null && lowStart <= lowEnd)
            {
                result.Add(new Customer
                {
                    CustomerId = lowNode.CustomerId,
                    Score = lowNode.Score,
                    Rank = lowStart
                });
                lowNode = lowNode.Next[0];
                lowStart++;
            }
        }

        return result;
    }

    public List<Customer> GetRange(int start, int end)
    {
        if (_count == 0 || start > end || start > _count) return [];

        var result = new List<Customer>(Math.Min(end - start + 1, Count));
        Node current = _head;
        int rank = 0;

        // Move to the node's start position
        for (var i = _level - 1; i >= 0; i--)
        {
            while (current.Next[i] != null && rank + current.Span[i] <= start)
            {
                rank += current.Span[i];
                current = current.Next[i];
            }
        }

        if (rank < start)
        {
            current = current.Next[0];
            rank++;
        }

        // Iterate until the end postion
        while (current != null && rank <= end)
        {
            result.Add(new Customer
            {
                CustomerId = current.CustomerId,
                Score = current.Score,
                Rank = rank
            });

            current = current.Next[0];
            rank++;
        }

        return result;
    }

    private bool TryRemove(long customerId, decimal score)
    {
        Node[] update = new Node[_maxLevel];
        Node current = _head;

        // Find the node before the deletion point
        for (var i = _level - 1; i >= 0; i--)
        {
            while (current.Next[i] != null &&
                  (current.Next[i].Score > score ||
                   (current.Next[i].Score == score && current.Next[i].CustomerId < customerId)))
            {
                current = current.Next[i];
            }
            update[i] = current;
        }

        Node target = current.Next[0];
        if (target == null || target.CustomerId != customerId)
            return false;

        // Update the pointers and spans for all levels
        for (int i = 0; i < _level; i++)
        {
            if (update[i].Next[i] == target)
            {
                update[i].Next[i] = target.Next[i];
                update[i].Span[i] += target.Span[i] - 1;
            }
            else if (update[i].Next[i] != null)
            {
                update[i].Span[i]--;
            }
        }

        Interlocked.Decrement(ref _count);
        return true;
    }

    private class AtomicLock
    {
        private volatile int _lockState = 0;

        public bool TryLock()
        {
            return Interlocked.CompareExchange(ref _lockState, 1, 0) == 0;
        }

        public void ExitLock()
        {
            Interlocked.Exchange(ref _lockState, 0);
        }
    }

    private class Node
    {
        public long CustomerId { get; }
        public decimal Score { get; set; }
        public Node[] Next { get; }
        public int[] Span { get; }
        public int Level
        {
            get
            {
                return CustomerId == 0
                    ? Next.Where(node => node != null).Count()
                    : Span.Length;
            }
        }

        public Node(long customerId, decimal score, int level)
        {
            CustomerId = customerId;
            Score = score;
            Next = new Node[level];
            Span = new int[level];
        }
    }
}