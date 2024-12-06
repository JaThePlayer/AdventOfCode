using System.Runtime.CompilerServices;

namespace AoC._2024;

/*
| Method | Mean     | Error   | StdDev  | Gen0    | Gen1   | Allocated |
|------- |---------:|--------:|--------:|--------:|-------:|----------:|
| Part1  | 123.7 us | 1.65 us | 1.54 us | 17.0898 | 4.6387 | 141.23 KB |
| Part2  | 126.0 us | 0.69 us | 0.61 us | 17.0898 | 4.6387 | 141.23 KB |

Spans
| Method | Mean     | Error   | StdDev  | Gen0    | Gen1   | Allocated |
|------- |---------:|--------:|--------:|--------:|-------:|----------:|
| Part1  | 120.3 us | 1.01 us | 0.84 us | 17.2119 | 4.6387 | 141.23 KB |
| Part2  | 125.6 us | 0.53 us | 0.49 us | 17.0898 | 4.6387 | 141.23 KB |

Provide guess for capacity when parsing lines
| Method | Mean     | Error   | StdDev  | Gen0    | Gen1   | Allocated |
|------- |---------:|--------:|--------:|--------:|-------:|----------:|
| Part1  | 107.2 us | 0.21 us | 0.19 us | 12.6953 | 3.4180 | 104.09 KB |
| Part2  | 111.5 us | 0.34 us | 0.28 us | 12.6953 | 3.4180 | 104.09 KB |

Starting capacity for the line list.
| Method | Mean     | Error   | StdDev  | Gen0    | Gen1   | Allocated |
|------- |---------:|--------:|--------:|--------:|-------:|----------:|
| Part1  | 101.8 us | 0.31 us | 0.26 us | 11.7188 | 3.4180 |  95.75 KB |
| Part2  | 107.8 us | 0.39 us | 0.35 us | 11.7188 | 3.4180 |  95.75 KB |

Don't parse up-front
| Method | Mean      | Error    | StdDev   | Allocated |
|------- |----------:|---------:|---------:|----------:|
| Part1  |  92.32 us | 0.210 us | 0.187 us |     120 B |
| Part2  | 100.23 us | 0.605 us | 0.472 us |     120 B |

ParseLines
| Method | Mean      | Error    | StdDev   | Allocated |
|------- |----------:|---------:|---------:|----------:|
| Part1  |  88.33 us | 0.269 us | 0.239 us |     120 B |
| Part2  | 100.35 us | 0.398 us | 0.373 us |     120 B |

| Method | Mean     | Error    | StdDev   | Allocated |
|------- |---------:|---------:|---------:|----------:|
| Part1  | 89.35 us | 0.208 us | 0.185 us |     120 B |
| Part2  | 95.44 us | 0.264 us | 0.247 us |     120 B |

Util.FastParseInt
| Method | Mean     | Error    | StdDev   | Allocated |
|------- |---------:|---------:|---------:|----------:|
| Part1  | 70.01 us | 0.240 us | 0.213 us |     120 B |
| Part2  | 73.85 us | 0.411 us | 0.364 us |     120 B |

Util.FastParseIntList
| Method | Mean     | Error    | StdDev   | Allocated |
|------- |---------:|---------:|---------:|----------:|
| Part1  | 23.06 us | 0.229 us | 0.214 us |     120 B |
| Part2  | 26.77 us | 0.152 us | 0.142 us |     120 B |


Array as the buffer
| Method | Mean     | Error    | StdDev   | Allocated |
|------- |---------:|---------:|---------:|----------:|
| Part1  | 20.90 us | 0.161 us | 0.150 us |     112 B |
| Part2  | 23.68 us | 0.152 us | 0.142 us |     112 B |
 */
public class Day02 : AdventBase
{
    public override int Year => 2024;
    public override int Day => 2;

    [InlineArray(16)]
    private struct IntBuffer
    {
        private int _first;
    }

    private LinqExt.SplitParser<char, Span<int>, Span<int>> ParseInput()
    {
        //var data = new IntBuffer();
        Span<int> buffer = new int[16];// new Span<int>(Unsafe.AsPointer(ref data[0]), 16);
        return Input.Text.AsSpan().ParseSplits('\n', buffer, static (line, buffer)
            => Util.FastParseIntList(line, ' ', buffer));
    }

    private static bool IsSafe(ReadOnlySpan<int> report, out int errorIdx)
    {
        var lastValue = report[0];
        var isIncreasing = report[1] > lastValue;
        var sign = isIncreasing ? 1 : -1;
        for (var i = 1; i < report.Length; i++)
        {
            var value = report[i];
            var diff = (value - lastValue) * sign;
            if (diff is <= 0 or > 3)
            {
                errorIdx = i;
                return false;
            }
            lastValue = value;
        }
        errorIdx = default;
        return true;
    }
    
    private static bool IsSafeWithErrorRemoved(ReadOnlySpan<int> report, int removedIdx)
    {
        var firstValueIdx = removedIdx == 0 ? 1 : 0;
        var lastValue = report[firstValueIdx];
        var isIncreasing = report[removedIdx is 0 or 1 ? 2 : 1] > lastValue;
        var sign = isIncreasing ? 1 : -1;
        for (var i = firstValueIdx+1; i < report.Length; i++)
        {
            if (i == removedIdx)
                continue;
            
            var value = report[i];
            var diff = (value - lastValue) * sign;
            if (diff is <= 0 or > 3)
                return false;
            
            lastValue = value;
        }
        return true;
    }
    
    protected override object Part1Impl()
    {
        var ret = 0;
        foreach (var report in ParseInput())
        {
            ret += IsSafe(report, out _) ? 1 : 0;
        }
        return ret; // 516
    }

    protected override object Part2Impl()
    {
        var ret = 0;
        foreach (var report in ParseInput())
        {
            if (IsSafe(report, out var firstErrorIdx))
            {
                ret++;
                continue;
            }
            // There are 3 ways a report can be saved:
            // * Fixing the first error fixes the report
            // * Removing the first or second item - changes the sorting direction, which might fix all subsequent errors.
            // Any other cases cannot be fixed - if there are 2 distance errors, we can only fix one.
            if (IsSafeWithErrorRemoved(report, firstErrorIdx))
            {
                ret++;
                continue;
            }
            // Try to see if changing sorting directions can work.
            // However, this only makes sense when the error occured early on -
            // otherwise, changing the sort order is guaranteed to create an error before we even get to the previous error index
            if (firstErrorIdx < 4 && (IsSafeWithErrorRemoved(report, 0) || IsSafeWithErrorRemoved(report, 1)))
            {
                ret++;
            }
        }
        return ret; // 561
    }
}