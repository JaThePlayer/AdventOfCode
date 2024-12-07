using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AoC._2024;

/*
Initial
| Method | Mean         | Error       | StdDev      | Gen0       | Allocated   |
|------- |-------------:|------------:|------------:|-----------:|------------:|
| Part1  |     871.0 us |     5.75 us |     5.38 us |          - |       304 B |
| Part2  | 443,411.0 us | 4,177.74 us | 3,907.86 us | 47000.0000 | 399183352 B |

Optimise Concat
| Method | Mean         | Error       | StdDev      | Allocated |
|------- |-------------:|------------:|------------:|----------:|
| Part1  |     875.4 us |     5.26 us |     4.66 us |     304 B |
| Part2  | 217,871.1 us | 1,307.09 us | 2,219.54 us |     341 B |

Optimise Concat again
| Method | Mean        | Error     | StdDev    | Allocated |
|------- |------------:|----------:|----------:|----------:|
| Part1  |    866.2 us |   4.09 us |   3.83 us |     304 B |
| Part2  | 46,895.8 us | 375.45 us | 351.19 us |     348 B |

TryOnlyOne: early return when addition is too large
| Method | Mean     | Error    | StdDev   | Allocated |
|------- |---------:|---------:|---------:|----------:|
| Part2  | 27.46 ms | 0.112 ms | 0.105 ms |     316 B |

TryDispatch: early return when addition is too large
| Method | Mean     | Error    | StdDev   | Allocated |
|------- |---------:|---------:|---------:|----------:|
| Part2  | 26.70 ms | 0.128 ms | 0.120 ms |     316 B |

Start recursing from the 2nd number instead of the 1st
| Method | Mean        | Error    | StdDev   | Allocated |
|------- |------------:|---------:|---------:|----------:|
| Part1  |    414.6 us |  1.57 us |  1.47 us |     144 B |
| Part2  | 12,184.9 us | 72.11 us | 67.46 us |     150 B |

Remove fast path in TryGetOne
| Method | Mean        | Error    | StdDev   | Allocated |
|------- |------------:|---------:|---------:|----------:|
| Part1  |    408.0 us |  1.51 us |  1.34 us |     144 B |
| Part2  | 11,660.2 us | 67.99 us | 63.60 us |     150 B |
*/
public class Day07 : AdventBase
{
    public override int Year => 2024;
    public override int Day => 7;

    protected static LinqExt.SplitParser<char, Span<ulong>, RefTuple<ulong, Span<ulong>>> Parse(ReadOnlySpan<char> input,
        Span<ulong> buffer)
    {
        return input.ParseSplits('\n', buffer, static (line, buffer) =>
        {
            line.SplitTwo(':', out var target, out var numbersSpan);
            var m = Util.FastParseIntList(numbersSpan[1..], ' ', buffer);
            
            return RefTuple.Create(Util.FastParseInt<ulong>(target), m);
        });
    }

    private static bool TryPart1(ulong target, ulong current, Span<ulong> numbers)
    {
        if (current > target)
            return false;

        return numbers switch
        {
            [] => target == current,
            [var only] => current + only == target || current * only == target,
            [var first, .. var remaining] =>
                TryPart1(target, current + first, remaining) 
             || TryPart1(target, current * first, remaining)
        };
    }
    
    protected override object Part1Impl()
    {
        var input = Input.Text.AsSpan();
        Span<ulong> buffer = new ulong[12];
        ulong sum = 0;

        foreach (var (num, numbers) in Parse(input, buffer))
        {
            sum += TryPart1(num, numbers[0], numbers[1..]) ? num : 0;
        }

        return sum; // 3351424677624
    }

    private static bool TryPart2(ulong target, ulong current, Span<ulong> numbers)
    {
        if (current > target)
            return false;

        return numbers switch
        {
            [] => target == current,
            [var only] => TryOnlyOne(target, current, only),
            [var first, .. var remaining] => TryDispatch(target, current, first, remaining)
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool TryOnlyOne(ulong target, ulong current, ulong only)
        {
            return current + only == target || current * only == target || current.Concat(only) == target;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool TryDispatch(ulong target, ulong current, ulong next, Span<ulong> remaining)
        {
            // Addition is too strong already, only option is that current == target and all remaining nums are 1.
            if (current + next > target)
            {
                return current == target && next == 1 && !remaining.ContainsAnyExcept((ulong)1);
            }

            return TryPart2(target, current + next, remaining)
                   || TryPart2(target, current * next, remaining)
                   || TryPart2(target, current.Concat(next), remaining);
        }
    }
    
    protected override object Part2Impl()
    {
        var input = Input.Text.AsSpan();
        Span<ulong> buffer = new ulong[12];
        ulong sum = 0;

        foreach (var (num, numbers) in Parse(input, buffer))
        {
            sum += TryPart2(num, numbers[0], numbers[1..]) ? num : 0;
        }

        return sum; // 204976636995111
    }
}