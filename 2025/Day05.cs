namespace AoC._2025;

/*
Initial
| Method | Mean      | Error    | StdDev   | Gen0   | Allocated |
|------- |----------:|---------:|---------:|-------:|----------:|
| Part1  | 111.04 us | 0.665 us | 0.622 us | 0.9766 |   8.16 KB |
| Part2  |  50.09 us | 0.807 us | 0.755 us | 2.0142 |  16.69 KB |
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

        var fresh = 0;
        while (lines.MoveNext())
        {
            var line = input[lines.Current];
            if (line.IsEmpty)
                break;
            
            var id = Util.FastParseInt<long>(line);

            foreach (var (s, e) in ranges)
            {
                if (s <= id && id <= e)
                {
                    fresh++;
                    break;
                }
                    
            }
        };

        return fresh;
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
        while (ranges.Count > 0)
        {
            var smallest = ranges.MinBy(x => x.start);
            prev = long.Max(prev, smallest.start);
            var amt = smallest.end - prev + 1;
            if (amt > 0)
                fresh += amt;
            prev = long.Max(prev, smallest.end + 1);
            ranges.Remove(smallest);
        }

        return fresh; // 353716783056994
    }
}