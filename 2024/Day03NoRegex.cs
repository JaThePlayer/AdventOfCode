using System.Buffers;

namespace AoC._2024;

/*
Baseline regex impl
| Method | Mean     | Error    | StdDev   | Gen0    | Gen1    | Allocated |
|------- |---------:|---------:|---------:|--------:|--------:|----------:|
| Part1  | 98.79 us | 1.884 us | 1.573 us | 44.4336 | 24.2920 |  363.8 KB |
| Part2  | 92.20 us | 1.026 us | 0.959 us | 38.4521 | 19.1650 | 313.84 KB |
 
Initial
| Method | Mean      | Error     | StdDev    | Allocated |
|------- |----------:|----------:|----------:|----------:|
| Part1  |  8.892 us | 0.0519 us | 0.0486 us |      24 B |
| Part2  | 15.381 us | 0.0389 us | 0.0364 us |      24 B |

Only search for commands that can impact the result in part2
| Method | Mean      | Error     | StdDev    | Allocated |
|------- |----------:|----------:|----------:|----------:|
| Part1  |  8.912 us | 0.0452 us | 0.0423 us |      24 B |
| Part2  | 10.264 us | 0.0209 us | 0.0185 us |      24 B |

GOTO
| Method | Mean     | Error     | StdDev    | Allocated |
|------- |---------:|----------:|----------:|----------:|
| Part1  | 8.380 us | 0.0737 us | 0.0689 us |      24 B |
| Part2  | 9.098 us | 0.0366 us | 0.0343 us |      24 B |

Slight ParseMul cleanup + part2: call StartsWith(d) instead of StartsWith(don't()) + skip through 4 more chars after a do() cmd 
| Method | Mean     | Error     | StdDev    | Allocated |
|------- |---------:|----------:|----------:|----------:|
| Part1  | 7.847 us | 0.1371 us | 0.1283 us |      24 B |
| Part2  | 8.940 us | 0.0186 us | 0.0174 us |      24 B |

 */
public class Day03NoRegex : Day03
{
    private static readonly SearchValues<string> Part1Search = SearchValues.Create(["mul("], StringComparison.Ordinal);
    private static readonly SearchValues<string> Part2SearchEnabled = SearchValues.Create(["mul(", "don't()"], StringComparison.Ordinal);
    private static readonly SearchValues<string> Part2SearchDisabled = SearchValues.Create(["do()"], StringComparison.Ordinal);
    
    private int ParseMul(ReadOnlySpan<char> mul, out int len)
    {
        // skip past mul(
        len = "mul(".Length;
        // longest valid string is 123,567)
        var slice = mul[len..(len + 8)];
        var a = 0;
        foreach (var c in slice)
        {
            len++;
            switch (c)
            {
                case ',':
                    goto findEndLoop;
                case >= '0' and <= '9':
                    a = a * 10 + (c - '0');
                    break;
                default:
                    return 0;
            }
        }
        return 0;
        
        findEndLoop:
        // longest valid string is 567)
        slice = mul[len..(len + 4)];
        var num = 0;
        foreach (var c in slice)
        {
            len++;
            switch (c)
            {
                case >= '0' and <= '9':
                    num = num * 10 + (c - '0');
                    break;
                case ')':
                    return a * num;
                default:
                    return 0;
            }
        }

        return 0;
    }
    
    protected override object Part1Impl()
    {
        var input = Input.Text.AsSpan();
        var sum = 0;
        int i;
        while ((i = input.IndexOfAny(Part1Search)) != -1)
        {
            sum += ParseMul(input[i..], out var len);
            input = input[(i + len)..];
        }
        return sum; // 166630675
    }

    protected override object Part2Impl()
    {
        var input = Input.Text.AsSpan();
        var sum = 0;
        int i;
        // Since output is enabled, search for don't() or mul()
        while ((i = input.IndexOfAny(Part2SearchEnabled)) != -1)
        {
            var sliced = input[i..];
            if (sliced.StartsWith('d'))
            {
                // this is definitely a "dont()" command
                input = sliced[7..];
                // Find the next do(), that's the only command that can change anything
                if ((i = input.IndexOfAny(Part2SearchDisabled)) == -1)
                    break;
                input = input[(i+4)..];
                continue;
            }
            sum += ParseMul(sliced, out var len);
            input = sliced[len..];
        }
        return sum; //93465710
    }
}