using System;
using System.Collections.Generic;
using System.Diagnostics;

class CodePerformanceProfiler
{
    static void Main()
    {
        Console.WriteLine("=== Code Performance Profiler ===");

        // Example test: measure how long it takes to count to 1 million
        int iterations = 10;       // Run the test 10 times
        int loopCount = 1_000_000;

        Func<int, long> testFunction = (arg) =>
        {
            long sum = 0;
            for (int i = 0; i < arg; i++)
            {
                sum += i;
            }
            return sum;
        };

        var stats = Profile(() => testFunction(loopCount), iterations);

        Console.WriteLine($"\nTest Function: Count to {loopCount:N0}");
        Console.WriteLine($"Runs: {iterations}");
        Console.WriteLine($"Min Time: {stats.Min:F2} ms");
        Console.WriteLine($"Max Time: {stats.Max:F2} ms");
        Console.WriteLine($"Average Time: {stats.Average:F2} ms");
    }

    public static PerformanceStats Profile(Action action, int iterations)
    {
        var stats = new PerformanceStats();

        for (int i = 0; i < iterations; i++)
        {
            GC.Collect();           // Reduce memory impact
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var sw = Stopwatch.StartNew();
            action();
            sw.Stop();

            long elapsedMs = sw.ElapsedMilliseconds;
            stats.Add(elapsedMs);
        }

        return stats;
    }

    public static PerformanceStats Profile(Func<long> function, int iterations)
    {
        var stats = new PerformanceStats();

        for (int i = 0; i < iterations; i++)
        {
            GC.Collect();           // Reduce memory impact
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var sw = Stopwatch.StartNew();
            function();              // Run the function
            sw.Stop();

            long elapsedMs = sw.ElapsedMilliseconds;
            stats.Add(elapsedMs);
        }

        return stats;
    }
}

public class PerformanceStats
{
    private List<long> _times = new List<long>();

    public long Min
    {
        get
        {
            if (_times.Count == 0) return 0;
            long min = _times[0];
            foreach (var t in _times)
            {
                if (t < min) min = t;
            }
            return min;
        }
    }

    public long Max
    {
        get
        {
            if (_times.Count == 0) return 0;
            long max = _times[0];
            foreach (var t in _times)
            {
                if (t > max) max = t;
            }
            return max;
        }
    }

    public double Average
    {
        get
        {
            if (_times.Count == 0) return 0;
            long total = 0;
            foreach (var t in _times)
            {
                total += t;
            }
            return (double)total / _times.Count;
        }
    }

    public void Add(long milliseconds)
    {
        _times.Add(milliseconds);
    }
}