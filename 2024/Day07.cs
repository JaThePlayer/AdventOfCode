using System.Globalization;
using System.Numerics;

namespace AoC._2024;

/*
Initial
| Method | Mean         | Error       | StdDev      | Gen0       | Allocated   |
|------- |-------------:|------------:|------------:|-----------:|------------:|
| Part1  |     871.0 us |     5.75 us |     5.38 us |          - |       304 B |
| Part2  | 443,411.0 us | 4,177.74 us | 3,907.86 us | 47000.0000 | 399183352 B |
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

    private bool TryPart1(ulong target, ulong current, Span<ulong> numbers)
    {
        if (current > target)
            return false;

        return numbers switch
        {
            [] => target == current,
            [var only] => current + only == target || current * only == target,
            [var first, .. var remaining] => TryPart1(target, current + first, remaining) 
                                                          || TryPart1(target, current * first, remaining)
        };
    }
    
    protected override object Part1Impl()
    {
        var input = Input.Text.AsSpan();
        var buffer = new ulong[32];
        ulong sum = 0;

        foreach (var (num, numbers) in Parse(input, buffer))
        {
            sum += TryPart1(num, 0, numbers) ? num : 0;
        }

        return sum; // 3351424677624
    }

    private static T Concat<T>(T left, T right) where T : INumber<T>
    {
        return T.Parse($"{left}{right}", CultureInfo.InvariantCulture);
    }

    private static bool TryPart2(ulong target, ulong current, Span<ulong> numbers)
    {
        if (current > target)
            return false;

        return numbers switch
        {
            [] => target == current,
            [var first, .. var remaining] => 
                TryPart2(target, current + first, remaining) 
             || TryPart2(target, current * first, remaining)
             || TryPart2(target, Concat(current, first), remaining)
        };
    }
    
    protected override object Part2Impl()
    {
        var input = Input.Text.AsSpan();
        var buffer = new ulong[32];
        ulong sum = 0;

        foreach (var (num, numbers) in Parse(input, buffer))
        {
            sum += TryPart2(num, 0, numbers) ? num : 0;
        }

        return sum; // 204976636995111
    }
}