namespace AoC._2024;

/*
Thanks to Simon on Celestecord for the idea.
 
| Method | Mean      | Error    | StdDev   | Allocated |
|------- |----------:|---------:|---------:|----------:|
| Part1  |  83.94 us | 1.595 us | 1.706 us |     144 B |
| Part2  | 115.19 us | 2.229 us | 2.085 us |     144 B |
 */
public class Day07Backwards : Day07
{
    private static bool TryPart1(ulong target, Span<ulong> numbers)
    {
        return numbers switch
        {
            [] => false,
            [var only] => only == target,
            [.. var remaining, var last] 
                when ulong.DivRem(target, last) is (var q, 0) && TryPart1(q, remaining) => true,
            [.. var remaining, var last] => TryPart1(target - last, remaining),
        };
    }
    
    protected override object Part1Impl()
    {
        var input = Input.Text.AsSpan();
        Span<ulong> buffer = new ulong[12];
        ulong sum = 0;

        foreach (var (num, numbers) in Parse(input, buffer))
        {
            sum += TryPart1(num, numbers) ? num : 0;
        }

        return sum; // 3351424677624
    }

    private static bool TryPart2(ulong target, Span<ulong> numbers)
    {
        return numbers switch
        {
            [] => false,
            [var only] => only == target,
            [.. var remaining, var last]
                when ulong.DivRem(target, last) is (var q, 0) && TryPart2(q, remaining) => true,
            [.. var remaining, var last]
                when ulong.DivRem(target - last, last.NextPowerOf10()) is (var q, 0) && TryPart2(q, remaining) => true,
            [.. var remaining, var last] => TryPart2(target - last, remaining),
        };
    }
    
    protected override object Part2Impl()
    {
        var input = Input.Text.AsSpan();
        Span<ulong> buffer = new ulong[12];
        ulong sum = 0;

        foreach (var (num, numbers) in Parse(input, buffer))
        {
            sum += TryPart2(num, numbers) ? num : 0;
        }

        return sum; // 204976636995111
    }
}