using CommunityToolkit.HighPerformance;

namespace AoC._2025;

/*
P1 alt (slower): merge ranges and binary search:
| Method | Mean     | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|------- |---------:|---------:|---------:|-------:|-------:|----------:|
| Part1  | 27.88 us | 0.323 us | 0.287 us | 3.4790 | 0.0610 |  28.57 KB |

Part 2 is trivially slower this way than the main solution, as range merging requires a full loop through all the ranges,
which in the main solution is sufficient to provide the answer already.
 */
public class Day05_RangeMerging : Day05
{
    protected override object Part1Impl()
    {
        var input = Input.TextU8;
        var ranges = ParseRanges(out var lines);
        
        ranges.Sort((a, b) => a.start.CompareTo(b.start));
        ranges = MergeRanges(ranges);
        var fresh = 0;

        List<long> ids = [];
        while (lines.MoveNext())
            ids.Add(Util.FastParseInt<long>(input[lines.Current]));
        ids.Sort();
        foreach (var (s, e) in ranges)
        {
            var startIdx = ids.BinarySearch(s);
            if (startIdx < 0)
                startIdx = ~startIdx;
            var endIdx = ids.BinarySearch(e);
            if (endIdx < 0)
                endIdx = ~endIdx;

            fresh += endIdx - startIdx;
        }
        
        return fresh;
    }

    List<(long start, long end)> MergeRanges(List<(long start, long end)> ranges)
    {
        List<(long start, long end)> newRanges = [ ranges[0] ];
        foreach (var range in ranges.AsSpan()[1..])
        {
            ref var last = ref newRanges.AsSpan()[^1];

            if (last.end < range.start)
            {
                newRanges.Add(range);
                continue;
            }

            last.end = long.Max(range.end, last.end);
        }

        return newRanges;
    }
}