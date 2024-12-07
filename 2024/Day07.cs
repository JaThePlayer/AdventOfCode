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
*/
public class Day07 : AdventBase
{
    public override int Year => 2024;
    public override int Day => 7;

    private static LinqExt.SplitParser<char, Span<ulong>, RefTuple<ulong, Span<ulong>>> Parse(ReadOnlySpan<char> input,
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

        static bool TryOnlyOne(ulong target, ulong current, ulong only)
        {
            // if addition is too strong, the only option is that 'current == target' and 'only' is 1.
            if (current + only > target)
                return current * only == target;
            
            return current + only == target || current * only == target || Concat(current, only) == target;
        }
        
        static bool TryDispatch(ulong target, ulong current, ulong next, Span<ulong> remaining)
        {
            // Addition is too strong already, only option is that current == target and all remaining nums are 1.
            if (current + next > target)
            {
                return current == target && next == 1 && !remaining.ContainsAnyExcept((ulong)1);
            }

            return TryPart2(target, current + next, remaining)
                   || TryPart2(target, current * next, remaining)
                   || TryPart2(target, Concat(current, next), remaining);
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

    // From System.Buffers.Text.FormattingHelpers.CountDigits
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int CountDigits(ulong value)
    {
        // Map the log2(value) to a power of 10.
        ReadOnlySpan<byte> log2ToPow10 =
        [
            1, 1, 1, 2, 2, 2, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5,
            6, 6, 6, 7, 7, 7, 7, 8, 8, 8, 9, 9, 9, 10, 10, 10,
            10, 11, 11, 11, 12, 12, 12, 13, 13, 13, 13, 14, 14, 14, 15, 15,
            15, 16, 16, 16, 16, 17, 17, 17, 18, 18, 18, 19, 19, 19, 19, 20
        ];

        // TODO: Replace with log2ToPow10[BitOperations.Log2(value)] once https://github.com/dotnet/runtime/issues/79257 is fixed
        uint index = Unsafe.Add(ref MemoryMarshal.GetReference(log2ToPow10), BitOperations.Log2(value));

        // Read the associated power of 10.
        ReadOnlySpan<ulong> powersOf10 =
        [
            0, // unused entry to avoid needing to subtract
            0,
            10,
            100,
            1000,
            10000,
            100000,
            1000000,
            10000000,
            100000000,
            1000000000,
            10000000000,
            100000000000,
            1000000000000,
            10000000000000,
            100000000000000,
            1000000000000000,
            10000000000000000,
            100000000000000000,
            1000000000000000000,
            10000000000000000000,
        ];
        ulong powerOf10 = Unsafe.Add(ref MemoryMarshal.GetReference(powersOf10), index);

        // Return the number of digits based on the power of 10, shifted by 1
        // if it falls below the threshold.
        bool lessThan = value < powerOf10;
        return (int)(index - Unsafe.As<bool, byte>(ref lessThan)); // while arbitrary bools may be non-0/1, comparison operators are expected to return 0/1
    }
    
    private static ulong Concat(ulong left, ulong right)
    {
        ReadOnlySpan<ulong> digitsToPowOf10 =
        [
            0,
            10,
            100,
            1000,
            10000,
            100000,
            1000000,
            10000000,
            100000000,
            1000000000,
            10000000000,
            100000000000,
            1000000000000,
            10000000000000,
            100000000000000,
            1000000000000000,
            10000000000000000,
            100000000000000000,
            1000000000000000000,
            10000000000000000000,
        ];

        return left * Unsafe.Add(ref MemoryMarshal.GetReference(digitsToPowOf10), CountDigits(right)) + right;
    }
}