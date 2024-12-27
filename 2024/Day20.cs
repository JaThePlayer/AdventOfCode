using System.Runtime.InteropServices;
using System.Text.Json;
using CommunityToolkit.HighPerformance;

namespace AoC._2024;

/*
Initial
| Method | Mean         | Error     | StdDev    | Gen0   | Gen1   | Allocated |
|------- |-------------:|----------:|----------:|-------:|-------:|----------:|
| Part1  |     88.61 us |  1.099 us |  0.974 us | 9.5215 | 2.3193 |  78.26 KB |
| Part2  | 36,653.60 us | 58.997 us | 46.061 us |      - |      - |  78.27 KB |
 */
public class Day20 : AdventBase
{
    public override int Year => 2024;
    public override int Day => 20;
    
    private void CalculateScores(ReadOnlySpan2D<byte> map, Span2D<int> scores, int x, int y)
    {
        var score = 0;
        Direction lastDir = Direction.None;

        while (map.DangerousGetReferenceAt(y, x) != 'E')
        {
            scores.DangerousGetReferenceAt(y, x) = score;
            score++;
            if (lastDir != Direction.Left && map.DangerousGetReferenceAt(y, x + 1) != '#')
            {
                x++;
                lastDir = Direction.Right;
            }
            else if (lastDir != Direction.Right && map.DangerousGetReferenceAt(y, x - 1) != '#')
            {
                x--;
                lastDir = Direction.Left;
            } else if (lastDir != Direction.Down && map.DangerousGetReferenceAt(y - 1, x) != '#')
            {
                y--;
                lastDir = Direction.Up;
            }
            else if (lastDir != Direction.Up)
            {
                y++;
                lastDir = Direction.Down;
            }
        }
        scores.DangerousGetReferenceAt(y, x) = score;
    }
    
    private int FindCheats(ReadOnlySpan2D<byte> map, Span2D<int> scores, int x, int y)
    {
        //Dictionary<int, int> cheats = new Dictionary<int, int>();
        
        var score = 0;
        Direction lastDir = Direction.None;
        var cheatCount = 0;

        while (map.DangerousGetReferenceAt(y, x) != 'E')
        {
            // Find cheat by walking into all possible walls
            
            // right
            if (map.DangerousGetReferenceAt(y, x + 1) == '#')
            {
                cheatCount += CountCheats(map, scores, y, x + 1, Direction.Right);
            }
            if (map.DangerousGetReferenceAt(y, x - 1) == '#')
            {
                cheatCount += CountCheats(map, scores, y, x - 1, Direction.Left);
            }
            if (map.DangerousGetReferenceAt(y + 1, x) == '#')
            {
                cheatCount += CountCheats(map, scores, y + 1, x, Direction.Down);
            }
            if (map.DangerousGetReferenceAt(y - 1, x) == '#')
            {
                cheatCount += CountCheats(map, scores, y - 1, x, Direction.Up);
            }
            
            // Next step:
            score++;
            if (lastDir != Direction.Left && map.DangerousGetReferenceAt(y, x + 1) != '#')
            {
                x++;
                lastDir = Direction.Right;
            }
            else if (lastDir != Direction.Right && map.DangerousGetReferenceAt(y, x - 1) != '#')
            {
                x--;
                lastDir = Direction.Left;
            } else if (lastDir != Direction.Down && map.DangerousGetReferenceAt(y - 1, x) != '#')
            {
                y--;
                lastDir = Direction.Up;
            }
            else if (lastDir != Direction.Up)
            {
                y++;
                lastDir = Direction.Down;
            }
        }

        //Console.WriteLine(JsonSerializer.Serialize(cheats));
        
        return cheatCount;

        int CountCheats(ReadOnlySpan2D<byte> map, Span2D<int> scores, int y, int x, Direction lastDir)
        {
            var c = 0;
            int saved;

            if (lastDir != Direction.Left && x + 1 < map.Width)
            {
                Visit(scores, x + 1, y);
            }
            if (lastDir != Direction.Up && y + 1 < map.Height)
            {
                Visit(scores, x, y + 1);
            }
            if (lastDir != Direction.Down && y > 0)
            {
                Visit(scores, x, y - 1);
            }
            if (lastDir != Direction.Right && x > 0)
            {
                Visit(scores, x - 1, y);
            }

            void Visit(Span2D<int> scores, int x, int y)
            {
                saved = scores.DangerousGetReferenceAt(y, x) - score - 2;
                if (saved >= 100)
                {
                    c++;
                }
            }
            return c;
        }
    }
    
    protected override object Part1Impl()
    {
        var map = Input.Create2DMap();
        var scores1d = new int[map.Width * map.Height];
        var scores = Span2D<int>.DangerousCreate(ref scores1d[0], map.Height, map.Width, 0);
        scores.Fill(-1);

        var (sx, sy) = map.IndexOf2D((byte)'S');
        CalculateScores(map, scores, sx, sy);
        
        return FindCheats(map, scores, sx, sy); // 1384
    }
    
    protected override object Part2Impl()
    {
        var map = Input.Create2DMap();
        var scores1d = new int[map.Width * map.Height];
        var scores = Span2D<int>.DangerousCreate(ref scores1d[0], map.Height, map.Width, 0);
        scores.Fill(-666);

        var (sx, sy) = map.IndexOf2D((byte)'S');

        CalculateScores(map, scores, sx, sy);
        
        return FindCheatsPart2(map, scores, sx, sy); // 1008542
    }
    
    private int FindCheatsPart2(ReadOnlySpan2D<byte> map, Span2D<int> scores, int x, int y)
    {
        var score = 0;
        Direction lastDir = Direction.None;
        var cheatCount = 0;

        while (map.DangerousGetReferenceAt(y, x) != 'E')
        {
            for (var nx = int.Max(0, x - 20); nx < int.Min(map.Width - 1, x + 21); nx++)
            {
                for (var ny = int.Max(0, y - 20); ny < int.Min(map.Height, y + 21); ny++)
                {
                    if (map.DangerousGetReferenceAt(ny, nx) == '#')
                        continue;
                    var dist = int.Abs(nx - x)  + int.Abs(ny - y);
                    if (dist > 20)
                        continue;
                    
                    var newScore = scores.DangerousGetReferenceAt(ny, nx);
                    var saved = newScore - score - dist;
                    if (saved >= 100)
                        cheatCount++;
                }
            }
            
            
            // Next step:
            score++;
            if (lastDir != Direction.Left && map.DangerousGetReferenceAt(y, x + 1) != '#')
            {
                x++;
                lastDir = Direction.Right;
            }
            else if (lastDir != Direction.Right && map.DangerousGetReferenceAt(y, x - 1) != '#')
            {
                x--;
                lastDir = Direction.Left;
            } else if (lastDir != Direction.Down && map.DangerousGetReferenceAt(y - 1, x) != '#')
            {
                y--;
                lastDir = Direction.Up;
            }
            else if (lastDir != Direction.Up)
            {
                y++;
                lastDir = Direction.Down;
            }
        }
        
        return cheatCount; // 1008542
    }
}