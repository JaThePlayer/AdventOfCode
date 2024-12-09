using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;

namespace AoC._2024;

/*

Initial
| Method | Mean     | Error     | StdDev    | Gen0   | Gen1   | Allocated |
|------- |---------:|----------:|----------:|-------:|-------:|----------:|
| Part1  | 4.631 us | 0.0878 us | 0.0733 us | 1.3580 | 0.0229 |  11.15 KB |
| Part2  | 7.736 us | 0.1050 us | 0.0982 us | 1.3580 | 0.0229 |  11.15 KB |

stackalloc the map
| Method | Mean     | Error     | StdDev    | Gen0   | Gen1   | Allocated |
|------- |---------:|----------:|----------:|-------:|-------:|----------:|
| Part1  | 4.281 us | 0.0752 us | 0.0703 us | 1.0605 | 0.0305 |   8.66 KB |
| Part2  | 6.453 us | 0.0750 us | 0.0701 us | 1.0605 | 0.0305 |   8.66 KB |

Initial capacities
| Method | Mean     | Error     | StdDev    | Gen0   | Gen1   | Allocated |
|------- |---------:|----------:|----------:|-------:|-------:|----------:|
| Part1  | 3.727 us | 0.0279 us | 0.0233 us | 0.7210 | 0.0114 |   5.89 KB |
| Part2  | 5.994 us | 0.0474 us | 0.0420 us | 0.7172 | 0.0076 |   5.89 KB |

Arrays, spans
| Method | Mean     | Error     | StdDev    | Gen0   | Gen1   | Allocated |
|------- |---------:|----------:|----------:|-------:|-------:|----------:|
| Part1  | 2.809 us | 0.0454 us | 0.0425 us | 0.5798 | 0.0076 |   4.76 KB |
| Part2  | 6.141 us | 0.0645 us | 0.0572 us | 0.7172 | 0.0076 |   5.89 KB |

ISolver
| Method | Mean     | Error     | StdDev    | Gen0   | Gen1   | Allocated |
|------- |---------:|----------:|----------:|-------:|-------:|----------:|
| Part1  | 2.995 us | 0.0270 us | 0.0239 us | 0.5798 | 0.0076 |   4.76 KB |
| Part2  | 4.294 us | 0.0676 us | 0.0632 us | 0.5798 | 0.0076 |   4.76 KB |
 */
public class Day08 : AdventBase
{
    public override int Year => 2024;
    public override int Day => 8;

    private interface ISolver
    {
        public void OnPair(int ax, int ay, int bx, int by, Span2D<bool> map);
        
        public int Result { get; }
    }

    private int Solve<T>() where T : ISolver, new()
    {
        T solver = new();
        var input = Input.Text.AsSpan();
        var lineWidth = input.IndexOf('\n') + 1;
        var span = ReadOnlySpan2D<char>.DangerousCreate(input[0], input.Length / lineWidth + 1, lineWidth, 0);
        
        Span<bool> map1d = stackalloc bool[(span.Width - 1) * span.Height];
        var map = Span2D<bool>.DangerousCreate(ref map1d[0], input.Length / lineWidth + 1, lineWidth - 1, 0);
        
        var antennas2 = ParseV2(span);
        foreach (var list in antennas2)
        {
            if (list is null)
                continue;

            var listSpan = CollectionsMarshal.AsSpan(list);
            for (var i = 0; i < listSpan.Length; i++)
            {
                var (ax, ay) = listSpan[i];
                for (var j = i+1; j < listSpan.Length; j++)
                {
                    var (bx, by) = listSpan[j];
                    solver.OnPair(ax, ay, bx, by, map);
                    solver.OnPair(bx, by, ax, ay, map);
                }
            }
        }

        return solver.Result;
    }


    private struct Part1Solver : ISolver
    {
        public int Result { get; private set; }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnPair(int ax, int ay, int bx, int by, Span2D<bool> map)
        {
            var (dx, dy) = (bx - ax, by - ay);
            var (px, py) = (ax + dx * 2, ay + dy * 2);
            if ((uint)px >= map.Width || (uint)py >= map.Height)
                return;
            ref var m = ref map.DangerousGetReferenceAt(py, px);
            if (m)
                return;
            m = true;
            Result++;
        }
    }
    
    private struct Part2Solver : ISolver
    {
        public int Result { get; private set; }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnPair(int ax, int ay, int bx, int by, Span2D<bool> map)
        {
            var (dx, dy) = (bx - ax, by - ay);
            var (px, py) = (ax + dx, ay + dy);
            
            while ((uint)px < map.Width && (uint)py < map.Height)
            {
                ref var m = ref map.DangerousGetReferenceAt(py, px);
                if (!m)
                {
                    m = true;
                    Result++;
                }

                px += dx;
                py += dy;
            }
        }
    }
    
    protected override object Part1Impl()
    {
        return Solve<Part1Solver>(); // 318
    }

    protected override object Part2Impl()
    {
        return Solve<Part2Solver>(); // 1126
    }
    
    private static List<(int x, int y)>?[] ParseV2(ReadOnlySpan2D<char> span)
    {
        var antennas = new List<(int x, int y)>?['z' - '0' + 1];
        for (int y = 0; y < span.Height; y++)
        {
            var row = span.GetRowSpan(y);
            for (int x = 0; x < span.Width - 1; x++)
            {
                var c = row.DangerousGetReferenceAt(x);
                if (c != '.')
                {
                    ref var list = ref antennas[c - '0'];
                    list ??= new(4);
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