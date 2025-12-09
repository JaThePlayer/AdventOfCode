using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;

namespace AoC._2025;

/*
Initial
| Method | Mean     | Error   | StdDev  | Gen0   | Allocated |
|------- |---------:|--------:|--------:|-------:|----------:|
| Part1  | 331.8 us | 1.63 us | 1.53 us | 0.9766 |   8.21 KB |
| Part2  | 2 minutes 13 seconds                  | 100_000L * 100_000L => ~9,31 GiB
 */
public class Day09 : AdventBase
{
    public override int Year => 2025;

    public override int Day => 9;
    
    protected override object Part1Impl()
    {
        List<(int x, int y)> points = [];
        var input = Input.TextU8;
        Span<int> numberBuffer = stackalloc int[2];
        
        foreach (var lineRange in input.Split((byte)'\n'))
        {
            var line = input[lineRange];
            Util.FastParseIntList(line, (byte)',', numberBuffer);
            points.Add(new(numberBuffer[0], numberBuffer[1]));
        }

        long maxArea = -1;
        for (int i = 0; i < points.Count; i++)
        {
            var a = points[i];
            for (int j = 0; j < points.Count; j++)
            {
                var b = points[j];
                
                var area = (long.Abs(a.x - b.x) + 1) * (long.Abs(a.y - b.y) + 1);
                //Console.WriteLine((a, b, area));
                maxArea = long.Max(maxArea, area);
            }
        }

        return maxArea; // 4763040296
    }
    
    public long Get1dLoc(long x, long y, int gridWidth) {
        return x + y * gridWidth;
    }
    
    public Point Get2dLoc(int index, int gridWidth) {
        (int q, int r) = int.DivRem(index, gridWidth);
        
        return new(r, q);
    }
    
    protected override unsafe object Part2Impl()
    {
        const int gw = 100_000;
        var bits = (byte*)NativeMemory.AllocZeroed((nuint)(100_000L * 100_000L));
        
        List<(long X, long Y)> points = [];
        var input = Input.TextU8;
        Span<int> numberBuffer = stackalloc int[2];
        
        foreach (var lineRange in input.Split((byte)'\n'))
        {
            var line = input[lineRange];
            Util.FastParseIntList(line, (byte)',', numberBuffer);
            points.Add(new(numberBuffer[0], numberBuffer[1]));
        }

        var next = 0;
        for (int current=0; current<points.Count; current++) {

            // get next vertex in list
            // if we've hit the end, wrap around to 0
            next = current+1;
            if (next == points.Count) next = 0;

            var vc = points[current];    // c for "current"
            var vn = points[next];       // n for "next"

            if (vc.X != vn.X)
                for (var x = long.Min(vc.X, vn.X); x <= long.Max(vc.X, vn.X); x++)
                    bits[Get1dLoc(x, vc.Y, gw)] = 1;
            if (vc.Y != vn.Y)
                for (var y = long.Min(vc.Y, vn.Y); y <= long.Max(vc.Y, vn.Y); y++)
                    bits[Get1dLoc(vc.X, y, gw)] = 1;
        }

        Console.WriteLine($"[{DateTime.Now}] flood");
        FloodFill((int)points[0].X + 1, (int)points[0].Y - 1, 
            (x, y) => x>=0&&y>=0&&x<gw&&y<gw&& bits[Get1dLoc(x, y, gw)] == 0,
            set: (x, y) => bits[Get1dLoc(x, y, gw)] = 2);
        Console.WriteLine($"[{DateTime.Now}] flood end");

        for (int y = 0; y < 11; y++)
        {
            for (int x = 0; x < 20; x++)
            {
                Console.Write(bits[Get1dLoc(x, y, gw)] == 0 ? '.' : 'X');
            }

            Console.WriteLine();
        }
        
        long maxArea = -1;
        List<Vector2> points2d = points.Select(x => new Vector2(x.X, x.Y)).ToList();
        for (int i = 0; i < points.Count; i++)
        {
            var a = points[i];
            Console.WriteLine($"[{DateTime.Now}] Progress: {i}/{points.Count - 1}");
            for (int j = i + 1; j < points.Count; j++)
            {
                var b = points[j];
                var w = long.Abs(a.X - b.X) + 1;
                var h = long.Abs(a.Y - b.Y) + 1;
                
                var area = (w * h);
                if (area > maxArea 
                    && bits[Get1dLoc(b.X, a.Y, gw)] != 0 
                    && bits[Get1dLoc(a.X, b.Y, gw)] != 0
                    )
                {
                    var valid = true;
                    for (long x = long.Min(a.X, b.X); valid && x <= long.Max(a.X, b.X); x++)
                    for (long y = long.Min(a.Y, b.Y); y <= long.Max(a.Y, b.Y); y++)
                        if (bits[Get1dLoc(x, y, gw)] == 0)
                        {
                            valid = false;
                            break;
                        }

                    if (valid)
                    {
                        maxArea = long.Max(maxArea, area);
                        Console.WriteLine($"NEW BEST: {maxArea} [{w}, {h}]: {a} + {b}");
                    }
                }
            }
        }
        
        return maxArea; // 1396494456 // [90312, 15463]: (4658, 65554) + (94969, 50092) ... in 2 minutes
    }
    
    // https://en.wikipedia.org/wiki/Flood_fill#Span_filling
    /// <summary>
    /// Performs a generalized flood fill, starting from <paramref name="sx"/>, <paramref name="sy"/>.
    /// </summary>
    /// <param name="sx">Starting x point of the fill</param>
    /// <param name="sy">Starting y point of the fill</param>
    /// <param name="inside">Returns true for unfilled points that should be inside the filled area. If <paramref name="set"/> got called on this tile, this should return false!</param>
    /// <param name="set">Fills a pixel/node. Any point that has Set called on it must then no longer be Inside.</param>
    /// <param name="cap">The limit on how many tiles can be flood filled at once</param>
    /// <returns>Whether the entire area got flooded. Returns false if the <paramref name="cap"/> got reached.</returns>
    public static bool FloodFill(int sx, int sy, Func<int, int, bool> inside, Action<int, int> set, int? cap = null) {
        if (!inside(sx, sy))
            return true;

        var count = 0;
        var s = new Queue<(int, int, int, int)>();
        
        s.Enqueue((sx, sx, sy, 1));
        s.Enqueue((sx, sx, sy - 1, -1));

        while (s.Count != 0) {
            var (x1, x2, y, dy) = s.Dequeue();
            var x = x1;

            if (inside(x, y)) {
                while (inside(x - 1, y)) {
                    set(x - 1, y);
                    count++;
                    if (cap < count) {
                        return false;
                    }
                    x -= 1;
                }
                
                if (x < x1) {
                    s.Enqueue((x, x1 - 1, y - dy, -dy));
                }
            }

            while (x1 <= x2) {
                while (inside(x1, y)) {
                    set(x1, y);
                    count++;
                    if (cap < count) {
                        return false;
                    }
                    x1 += 1;
                }

                if (x1 > x) {
                    s.Enqueue((x, x1 - 1, y + dy, dy));
                }

                if (x1 - 1 > x2) {
                    s.Enqueue((x2 + 1, x1 - 1, y - dy, -dy));
                }

                x1 += 1;

                while (x1 < x2 && !inside(x1, y)) {
                    x1 += 1;
                }
                x = x1;
            }
        }

        return true;
    }
}