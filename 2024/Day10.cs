using CommunityToolkit.HighPerformance;

namespace AoC._2024;

/*
Initial
| Method | Mean     | Error    | StdDev   | Allocated |
|------- |---------:|---------:|---------:|----------:|
| Part1  | 54.77 us | 1.082 us | 2.282 us |      24 B |
| Part2  | 35.80 us | 0.350 us | 0.328 us |      24 B |

Part1: use id's to avoid clearing the `visited` grid.
| Method | Mean     | Error    | StdDev   | Allocated |
|------- |---------:|---------:|---------:|----------:|
| Part1  | 31.77 us | 0.556 us | 0.595 us |      24 B |
| Part2  | 35.41 us | 0.265 us | 0.248 us |      24 B |

Part2: memoize
| Method | Mean     | Error    | StdDev   | Allocated |
|------- |---------:|---------:|---------:|----------:|
| Part1  | 30.68 us | 0.066 us | 0.052 us |      24 B |
| Part2  | 18.95 us | 0.263 us | 0.246 us |      24 B |
 */
public class Day10 : AdventBase
{
    public override int Year => 2024;
    public override int Day => 10;

    private static int Count(ReadOnlySpan2D<char> span, Span2D<ushort> visited, ushort id, char v, int sx, int sy)
    {
        ref var visit = ref visited.DangerousGetReferenceAt(sy, sx);
        if (visit == id)
            return 0;
        visit = id;
        if (v == '9')
            return 1;
        
        v += (char)1;
        var r = 0;
        var row = span.GetRowSpan(sy);

        if (sx > 0 && row.DangerousGetReferenceAt(sx-1) == v)
            r += Count(span, visited, id, v, sx-1, sy);
        if (row.DangerousGetReferenceAt(sx+1) == v)
            r += Count(span, visited, id, v, sx+1, sy);
        if (sy > 0 && span.DangerousGetReferenceAt(sy-1, sx) == v)
            r += Count(span, visited, id, v, sx, sy-1);
        if (sy+1 < span.Height && span.DangerousGetReferenceAt(sy+1, sx) == v)
            r += Count(span, visited, id, v, sx, sy+1);

        return r;
    }
    
    private static int Count2(ReadOnlySpan2D<char> span, Span2D<short> visited, char v, int sx, int sy)
    {
        ref var cache = ref visited.DangerousGetReferenceAt(sy, sx);
        if (cache > 0)
            return (short)(cache - 1);

        if (v == '9')
        {
            cache = 2;
            return 1;
        }

        v += (char)1;
        var res = 0;
        if (sx > 0 && span.DangerousGetReferenceAt(sy, sx-1) == v)
            res += Count2(span, visited, v, sx-1, sy);
        if (sy > 0 && span.DangerousGetReferenceAt(sy-1, sx) == v)
            res += Count2(span, visited, v, sx, sy-1);
        if (sx+1 < span.Width && span.DangerousGetReferenceAt(sy, sx+1) == v)
            res += Count2(span, visited, v, sx+1, sy);
        if (sy+1 < span.Height && span.DangerousGetReferenceAt(sy+1, sx) == v)
            res += Count2(span, visited, v, sx, sy+1);

        cache = (short)(res + 1);

        return res;
    }
    
    protected override object Part1Impl()
    {
        var sum = 0;
        var input = Input.Text.AsSpan();
        var lineWidth = input.IndexOf('\n') + 1;
        
        var span = ReadOnlySpan2D<char>.DangerousCreate(input[0], input.Length / lineWidth + 1, lineWidth, 0);
        
        // we'll store the ID of the starting point in the array, so we don't have to clear it each time
        Span<ushort> visited1D = stackalloc ushort[span.Height * span.Width];
        var visited = Span2D<ushort>.DangerousCreate(ref visited1D[0], span.Height, span.Width, 0);

        ushort id = 1;
        int i;
        var si = 0;
        while ((i = input.IndexOf('0')) != -1)
        {
            si += i;
            var x = si % span.Width;
            var y = si / span.Width;
            sum += Count(span, visited, id++, '0', x, y);
            
            input = input[(i + 1)..];
            si++;
        }
        
        return sum; // 682
    }

    protected override object Part2Impl()
    {
        var sum = 0;
        var input = Input.Text.AsSpan();
        var lineWidth = input.IndexOf('\n') + 1;
        
        var span = ReadOnlySpan2D<char>.DangerousCreate(input[0], input.Length / lineWidth + 1, lineWidth, 0);
        Span<short> visited1D = stackalloc short[span.Height * span.Width];
        var visited = Span2D<short>.DangerousCreate(ref visited1D[0], span.Height, span.Width, 0);

        int i;
        var si = 0;
        while ((i = input.IndexOf('0')) != -1)
        {
            si += i;
            var x = si % span.Width;
            var y = si / span.Width;
            sum += Count2(span, visited, '0', x, y);
            
            input = input[(i + 1)..];
            si++;
        }
        
        return sum; // 1511
    }
}