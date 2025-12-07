namespace AoC._2025;

/*
| Method | Mean      | Error    | StdDev   | Gen0    | Gen1    | Gen2    | Allocated |
|------- |----------:|---------:|---------:|--------:|--------:|--------:|----------:|
| Part1  |  22.57 us | 0.130 us | 0.115 us |  2.9297 |  0.2136 |       - |  24.09 KB |
| Part2  | 258.78 us | 5.008 us | 4.684 us | 90.8203 | 90.8203 | 90.8203 |  440.9 KB |

P2: low-hanging fruit
| Method | Mean     | Error   | StdDev  | Gen0    | Gen1    | Gen2    | Allocated |
|------- |---------:|--------:|--------:|--------:|--------:|--------:|----------:|
| Part2  | 111.7 us | 2.20 us | 2.36 us | 30.2734 | 30.2734 | 30.2734 | 210.62 KB |

P1: rework
| Method | Mean     | Error     | StdDev    | Gen0   | Gen1   | Allocated |
|------- |---------:|----------:|----------:|-------:|-------:|----------:|
| Part1  | 7.346 us | 0.0768 us | 0.0718 us | 2.4261 | 0.1831 |  19.91 KB |

P1: optimised rework
| Method | Mean     | Error     | StdDev    | Gen0   | Allocated |
|------- |---------:|----------:|----------:|-------:|----------:|
| Part1  | 3.001 us | 0.0279 us | 0.0261 us | 0.0229 |     192 B |

P2 rework
| Method | Mean     | Error     | StdDev    | Gen0   | Allocated |
|------- |---------:|----------:|----------:|-------:|----------:|
| Part1  | 2.980 us | 0.0059 us | 0.0049 us | 0.0229 |     192 B |
| Part2  | 3.335 us | 0.0224 us | 0.0210 us | 0.1373 |    1176 B |
 */
public class Day07 : AdventBase
{
    public override int Year => 2025;
    public override int Day => 07;
    
    protected override object Part1Impl()
    {
        var map = Input.Create2DMap();
        var splits = 0;
        
        var sx = map.IndexOf1D((byte)'S');

        Span<bool> beams = new bool[map.Width - 1];
        beams[sx] = true;
        for (var y = 2; y < map.Height; y += 2)
        {
            var row = map.GetRowSpan(y);
            for (var x = row.IndexOf((byte)'^'); x < row.Length - 1; x += 2)
            {
                if (row[x] != '^' || !beams[x])
                    continue;

                if (x > 0)
                    beams[x - 1] = true;
                if (x < map.Width - 2)
                    beams[x + 1] = true;
                beams[x] = false;
                splits++;
            }
        }

        return splits; // 1550
    }

    protected override object Part2Impl()
    {
        var map = Input.Create2DMap();
        long splits = 1;
        
        var sx = map.IndexOf1D((byte)'S');

        Span<long> beams = new long[map.Width - 1];
        beams[sx] = 1;
        for (var y = 2; y < map.Height; y += 2)
        {
            var row = map.GetRowSpan(y);
            for (var x = row.IndexOf((byte)'^'); x < row.Length - 1; x += 2)
            {
                if (row[x] != '^' || beams[x] <= 0)
                    continue;

                if (x > 0)
                    beams[x - 1] += beams[x];
                if (x < map.Width - 2)
                    beams[x + 1] += beams[x];
                splits += beams[x];
                beams[x] = 0;
            }
        }

        return splits; // 9897897326778
    }
}