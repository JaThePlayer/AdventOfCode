using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace AoC._2024;
/*
Initial (multithreaded)
| Method | Mean       | Error    | StdDev   | Gen0     | Gen1     | Gen2     | Allocated  |
|------- |-----------:|---------:|---------:|---------:|---------:|---------:|-----------:|
| Part1  |   204.9 us |  4.04 us |  3.77 us |  25.3906 |   6.5918 |        - |   196.9 KB |
| Part2  | 6,920.6 us | 66.15 us | 61.88 us | 593.7500 | 539.0625 | 296.8750 | 4821.22 KB |
 */

public class Day11 : AdventBase
{
    public override int Year => 2024;
    public override int Day => 11;

    private static ConcurrentDictionary<(ulong, int), ulong> _memory = new();
    
    private ulong SimulateRock(ulong rock, int maxSteps)
    {
        if (maxSteps == 0)
            return 1;
        if (_memory.TryGetValue((rock, maxSteps), out ulong result))
        {
            //Console.WriteLine("Using memo");
            return result;
        }

        ulong sum = 0;
        var initialMaxSteps = maxSteps;
        var initialRock = rock;

        while (maxSteps-- > 0)
        {
            if (rock == initialRock)
            {
                //Console.WriteLine($"LOOP: {initialMaxSteps}, {maxSteps}, {rock}");
            }
            
            if (rock == 0)
            {
                rock = 1;
            }
            else
            {
                var digits = rock.CountDigits();
                if (digits % 2 == 0)
                {
                    var powOf10 = MathExt.PowerOfTen(digits / 2);
                    var left = rock / powOf10;
                    var right = rock % powOf10;

                    rock = left;
                    sum += SimulateRock(right, maxSteps);
                }
                else
                {
                    rock *= 2024;
                }
            }
        }
        
        _memory.TryAdd((initialRock, initialMaxSteps), 1+sum);
        return 1 + sum;
    }
    
    private ulong Simulate(int maxSteps)
    {
        // For the sake of fair benchmarks, get rid of the cache
        _memory.Clear();
        
        var rocks = Input.Text.Split(' ').Select(x => (ulong.Parse(x), 1)).ToList();
        ulong sum = 0;

        Parallel.ForEach(rocks, r =>
        {
            Interlocked.Add(ref sum, (ulong)SimulateRock(r.Item1, maxSteps));
        });
        return sum;
    }
    
    protected override object Part1Impl()
    {
        return Simulate(25); // 229043
    }

    protected override object Part2Impl()
    {
        return Simulate(75); // 272673043446478
    }
}