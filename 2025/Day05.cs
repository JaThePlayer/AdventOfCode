namespace AoC._2025;

/*
Initial
| Method | Mean      | Error    | StdDev   | Gen0   | Allocated |
|------- |----------:|---------:|---------:|-------:|----------:|
| Part1  | 111.04 us | 0.665 us | 0.622 us | 0.9766 |   8.16 KB |
| Part2  |  50.09 us | 0.807 us | 0.755 us | 2.0142 |  16.69 KB |

P2: pre-sort the list
| Method | Mean     | Error     | StdDev    | Gen0   | Gen1   | Allocated |
|------- |---------:|----------:|----------:|-------:|-------:|----------:|
| Part2  | 6.337 us | 0.1248 us | 0.1167 us | 0.9918 | 0.0153 |   8.16 KB |

P1: new algo via sorting ranges and ids
| Method | Mean     | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|------- |---------:|---------:|---------:|-------:|-------:|----------:|
| Part1  | 25.22 us | 0.074 us | 0.062 us | 2.9602 | 0.1221 |  24.37 KB |
 */
public class Day05 : AdventBase
{
    public override int Year => 2025;
    public override int Day => 5;
    
    protected override object Part1Impl()
    {
        var input = Input.TextU8;
        var lines = input.Split((byte)'\n');
        List<(long start, long end)> ranges = [];
        while (lines.MoveNext())
        {
            var line = input[lines.Current];
            if (line.IsEmpty)
                break;
            line.ParseTwoSplits((byte)'-', Util.FastParseInt<long>, out var left, out var right);
            ranges.Add((left, right));
        }

        List<long> ids = [];
        while (lines.MoveNext())
        {
            var line = input[lines.Current];
            if (line.IsEmpty)
                break;
            ids.Add(Util.FastParseInt<long>(line));
        }
        
        ranges.Sort((a, b) => a.start.CompareTo(b.start));
        ids.Sort();

        var fresh = 0;
        var idIdx = 0;
        var id = ids[idIdx];
        var rangeIdx = 0;
        var (start, end) = ranges[rangeIdx];
        while (true)
        {
            while (id < start)
            {
                if (++idIdx >= ids.Count)
                    break;
                id = ids[idIdx];
            }

            if (id > end)
            {
                if (++rangeIdx >= ranges.Count)
                    break;
                (start, end) = ranges[rangeIdx];
                continue;
            }

            fresh++;
            if (++idIdx >= ids.Count)
                break;
            id = ids[idIdx];
        }

        return fresh; // 615
    }

    protected override object Part2Impl()
    {
        var input = Input.TextU8;

        var lines = input.Split((byte)'\n');
        List<(long start, long end)> ranges = [];
        while (lines.MoveNext())
        {
            var line = input[lines.Current];
            if (line.IsEmpty)
                break;
            line.ParseTwoSplits((byte)'-', Util.FastParseInt<long>, out var left, out var right);
            ranges.Add((left, right));
        }

        long fresh = 0;
        long prev = -1;
        ranges.Sort((a, b) => a.start.CompareTo(b.start));
        foreach (var (start, end) in ranges)
        {
            prev = long.Max(prev, start);
            var amt = end - prev + 1;
            if (amt > 0)
            {
                fresh += amt;
                prev = end + 1;
            }
        }

        return fresh; // 353716783056994
    }
}