using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace AoC._2024;
/*
Initial (multithreaded)
| Method | Mean       | Error    | StdDev   | Gen0     | Gen1     | Gen2     | Allocated  |
|------- |-----------:|---------:|---------:|---------:|---------:|---------:|-----------:|
| Part1  |   204.9 us |  4.04 us |  3.77 us |  25.3906 |   6.5918 |        - |   196.9 KB |
| Part2  | 6,920.6 us | 66.15 us | 61.88 us | 593.7500 | 539.0625 | 296.8750 | 4821.22 KB |

Singlethreaded with Dictionary
| Method | Mean        | Error     | StdDev    | Gen0     | Gen1     | Gen2     | Allocated |
|------- |------------:|----------:|----------:|---------:|---------:|---------:|----------:|
| Part1  |    54.84 us |  0.378 us |  0.316 us |   7.3853 |   1.2207 |        - |  60.84 KB |
| Part2  | 7,863.50 us | 43.567 us | 40.753 us | 515.6250 | 500.0000 | 500.0000 | 2461.4 KB |

Try to get memoized value on each iteration
| Method | Mean        | Error     | StdDev    | Gen0     | Gen1     | Gen2     | Allocated |
|------- |------------:|----------:|----------:|---------:|---------:|---------:|----------:|
| Part1  |    42.70 us |  0.523 us |  0.490 us |   7.3853 |   1.2207 |        - |  60.84 KB |
| Part2  | 4,898.18 us | 40.056 us | 37.468 us | 523.4375 | 500.0000 | 500.0000 | 2461.4 KB |

Presize the memo dictionary
| Method | Mean        | Error     | StdDev    | Gen0     | Gen1     | Gen2     | Allocated  |
|------- |------------:|----------:|----------:|---------:|---------:|---------:|-----------:|
| Part1  |    38.90 us |  0.143 us |  0.119 us |   4.0283 |   0.4272 |        - |   33.03 KB |
| Part2  | 4,113.44 us | 48.111 us | 42.649 us | 328.1250 | 328.1250 | 328.1250 | 1278.87 KB |

FastParseIntList instead of linq
| Method | Mean        | Error     | StdDev   | Gen0     | Gen1     | Gen2     | Allocated |
|------- |------------:|----------:|---------:|---------:|---------:|---------:|----------:|
| Part1  |    39.07 us |  0.137 us | 0.107 us |   3.9673 |   0.4272 |        - |  32.46 KB |
| Part2  | 4,021.85 us | 10.266 us | 9.101 us | 328.1250 | 328.1250 | 328.1250 | 1278.3 KB |
 */

public class Day11 : AdventBase
{
    public override int Year => 2024;
    public override int Day => 11;
    
    private ulong SimulateRock(ulong rock, int maxSteps, Dictionary<(ulong, int), ulong> memory)
    {
        if (maxSteps == 0)
            return 1;
        if (memory.TryGetValue((rock, maxSteps), out var result))
            return result;

        ulong sum = 0;
        var initialMaxSteps = maxSteps;
        var initialRock = rock;

        while (maxSteps-- > 0)
        {
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
                    sum += SimulateRock(rock % powOf10, maxSteps, memory);
                    rock /= powOf10;
                }
                else
                {
                    rock *= 2024;
                }
            }
            
            if (memory.TryGetValue((rock, maxSteps), out var r2))
            {
                memory.TryAdd((initialRock, initialMaxSteps), sum + r2);
                return sum + r2;
            }
        }
        
        memory.TryAdd((initialRock, initialMaxSteps), 1+sum);
        return 1 + sum;
    }
    
    private ulong Simulate(int maxSteps, int capacityHint)
    {
        Span<ulong> rocksBuffer = stackalloc ulong[16];
        var rocks = Util.FastParseIntList(Input.TextU8, (byte)' ', rocksBuffer);
        ulong sum = 0;

        /*
        Parallel.ForEach(rocks, r =>
        {
            var memory = new Dictionary<(ulong, int), ulong>(capacityHint);
            Interlocked.Add(ref sum, (ulong)SimulateRock(r, maxSteps, memory));
            //Console.WriteLine((memory.Count, memory.Capacity));
        });
        */
        var memory = new Dictionary<(ulong, int), ulong>(capacityHint);
        foreach (var r in rocks)
        {
            sum += SimulateRock(r, maxSteps, memory);
        }
        
        
        return sum;
    }
    
    protected override object Part1Impl()
    {
        return Simulate(25, 852); // 229043
    }

    protected override object Part2Impl()
    {
        return Simulate(75, 32542); // 272673043446478
    }
}