using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;

namespace AoC._2024;

/*

Initial
| Method | Mean     | Error     | StdDev    | Gen0   | Gen1   | Allocated |
|------- |---------:|----------:|----------:|-------:|-------:|----------:|
| Part1  | 4.631 us | 0.0878 us | 0.0733 us | 1.3580 | 0.0229 |  11.15 KB |
| Part2  | 7.736 us | 0.1050 us | 0.0982 us | 1.3580 | 0.0229 |  11.15 KB |
 */
public class Day08 : AdventBase
{
    public override int Year => 2024;
    public override int Day => 8;
    protected override object Part1Impl()
    {
        var sum = 0;

        var input = Input.Text.AsSpan();
        var lineWidth = input.IndexOf('\n') + 1;
        var span = ReadOnlySpan2D<char>.DangerousCreate(input[0], input.Length / lineWidth + 1, lineWidth, 0);
        
        var antennas = Parse(span);

        var map = new bool[span.Width - 1, span.Height];

        foreach (var (key, list) in antennas)
        {
            foreach (var (ax, ay) in list)
            {
                foreach (var (bx, by) in list)
                {
                    if (ax == bx && ay == by)
                        continue;
                    var (dx, dy) = (bx - ax, by - ay);
                    var (px, py) = (ax + dx*2, ay + dy*2);
                    if ((uint)px < span.Width - 1 && (uint)py < span.Height)
                    {
                        ref var m = ref map[px, py];
                        if (!m)
                        {
                            m = true;
                            sum++;
                        }
                    }
                }
            }
        }
        
        return sum; // 318
    }

    protected override object Part2Impl()
    {
        var sum = 0;

        var input = Input.Text.AsSpan();
        var lineWidth = input.IndexOf('\n') + 1;
        var span = ReadOnlySpan2D<char>.DangerousCreate(input[0], input.Length / lineWidth + 1, lineWidth, 0);
        
        var antennas = Parse(span);

        var map = new bool[span.Width - 1, span.Height];

        foreach (var (key, list) in antennas)
        {
            foreach (var (ax, ay) in list)
            {
                foreach (var (bx, by) in list)
                {
                    if (ax == bx && ay == by)
                        continue;
                    var (dx, dy) = (bx - ax, by - ay);
                    var (px, py) = (ax + dx, ay + dy);
                    
                    while ((uint)px < span.Width - 1 && (uint)py < span.Height)
                    {
                        ref var m = ref map[px, py];
                        if (!m)
                        {
                            m = true;
                            sum++;
                        }

                        px += dx;
                        py += dy;
                    }
                }
            }
        }

        return sum;
    }
    
    private static Dictionary<char, List<(int x, int y)>> Parse(ReadOnlySpan2D<char> span)
    {
        Dictionary<char, List<(int x, int y)>> antennas = new();
        for (int y = 0; y < span.Height; y++)
        {
            var row = span.GetRowSpan(y);
            for (int x = 0; x < span.Width - 1; x++)
            {
                var c = row.DangerousGetReferenceAt(x);
                if (c != '.')
                {
                    ref var list = ref CollectionsMarshal.GetValueRefOrAddDefault(antennas, c, out _);
                    list ??= new();
                    list.Add((x, y));
                }
            }
        }

        return antennas;
    }
    
/*
for (int y = 0; y < span.Height; y++)
{
    var row = span.GetRowSpan(y);
    for (int x = 0; x < span.Width - 1; x++)
    {
        var c = row.DangerousGetReferenceAt(x);
        var v = map[x, y];

        Console.Write(v ? '#' : c);
    }

    Console.WriteLine();
}
*/
}