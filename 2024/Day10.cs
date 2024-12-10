using CommunityToolkit.HighPerformance;

namespace AoC._2024;

/*
Initial
| Method | Mean     | Error    | StdDev   | Allocated |
|------- |---------:|---------:|---------:|----------:|
| Part1  | 54.77 us | 1.082 us | 2.282 us |      24 B |
| Part2  | 35.80 us | 0.350 us | 0.328 us |      24 B |
 */
public class Day10 : AdventBase
{
    public override int Year => 2024;
    public override int Day => 10;

    private static int Count(ReadOnlySpan2D<char> span, Span2D<bool> visited, char v, int r, int sx, int sy)
    {
        ref var visit = ref visited[sy, sx];
        if (visit)
            return r;
        visit = true;
        if (v == '9')
        {
            return r+1;
        }
        
        var t = (char)(v + 1);
        
        if (sx > 0 && span[sy, sx-1] == t)
            r = Count(span, visited, t, r, sx-1, sy);
        if (sy > 0 && span[sy-1, sx] == t)
            r = Count(span, visited, t, r, sx, sy-1);
        if (sx+1 < span.Width && span[sy, sx+1] == t)
            r = Count(span, visited, t, r, sx+1, sy);
        if (sy+1 < span.Height && span[sy+1, sx] == t)
            r = Count(span, visited, t, r, sx, sy+1);

        return r;
    }
    
    private static int Count2(ReadOnlySpan2D<char> span, char v, int r, int sx, int sy)
    {
        if (v == '9')
        {
            return r+1;
        }
        
        var t = (char)(v + 1);
        
        if (sx > 0 && span[sy, sx-1] == t)
            r = Count2(span, t, r, sx-1, sy);
        if (sy > 0 && span[sy-1, sx] == t)
            r = Count2(span, t, r, sx, sy-1);
        if (sx+1 < span.Width && span[sy, sx+1] == t)
            r = Count2(span, t, r, sx+1, sy);
        if (sy+1 < span.Height && span[sy+1, sx] == t)
            r = Count2(span, t, r, sx, sy+1);

        return r;
    }
    
    protected override object Part1Impl()
    {
        var sum = 0;
        var input = Input.Text.AsSpan();
        var lineWidth = input.IndexOf('\n') + 1;
        
        var span = ReadOnlySpan2D<char>.DangerousCreate(input[0], input.Length / lineWidth + 1, lineWidth, 0);
        Span<bool> visited1D = stackalloc bool[span.Height * span.Width];
        var visited = Span2D<bool>.DangerousCreate(ref visited1D[0], span.Height, span.Width, 0);

        for (var y = 0; y < span.Height; y++)
        {
            var row = span.GetRowSpan(y);

            for (var x = 0; x < span.Width; x++)
            {
                if (row[x] != '0')
                    continue;
                visited.Fill(false);
                checked
                {
                    var v = Count(span, visited, '0', 0, x, y);
                    sum += v;
                }
                
            }
        }
        
        return sum; // 682
    }

    protected override object Part2Impl()
    {
        var sum = 0;
        var input = Input.Text.AsSpan();
        var lineWidth = input.IndexOf('\n') + 1;
        
        var span = ReadOnlySpan2D<char>.DangerousCreate(input[0], input.Length / lineWidth + 1, lineWidth, 0);

        for (var y = 0; y < span.Height; y++)
        {
            var row = span.GetRowSpan(y);

            for (var x = 0; x < span.Width; x++)
            {
                if (row[x] != '0')
                    continue;
                checked
                {
                    var v = Count2(span, '0', 0, x, y);
                    sum += v;
                }
                
            }
        }
        
        return sum; // 1511
    }
}