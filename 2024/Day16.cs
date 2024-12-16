using System.Runtime.CompilerServices;
using CommunityToolkit.HighPerformance;

namespace AoC._2024;

/*
Initial
| Method | Mean     | Error    | StdDev   | Gen0    | Gen1    | Gen2    | Allocated |
|------- |---------:|---------:|---------:|--------:|--------:|--------:|----------:|
| Part1  | 15.51 ms | 0.101 ms | 0.079 ms | 31.2500 | 31.2500 | 31.2500 | 155.39 KB |
| Part2  | 14.51 ms | 0.064 ms | 0.057 ms | 93.7500 | 93.7500 | 93.7500 | 349.74 KB |

Part1: Use Visit2
| Method | Mean     | Error    | StdDev   | Gen0    | Gen1    | Gen2    | Allocated |
|------- |---------:|---------:|---------:|--------:|--------:|--------:|----------:|
| Part1  | 14.42 ms | 0.062 ms | 0.055 ms | 93.7500 | 93.7500 | 93.7500 |  330.3 KB |
| Part2  | 14.51 ms | 0.039 ms | 0.032 ms | 93.7500 | 93.7500 | 93.7500 | 349.74 KB |

GetCost<TLeft, TRight>
| Method | Mean     | Error    | StdDev   | Gen0    | Gen1    | Gen2    | Allocated |
|------- |---------:|---------:|---------:|--------:|--------:|--------:|----------:|
| Part1  | 13.41 ms | 0.051 ms | 0.043 ms | 93.7500 | 93.7500 | 93.7500 |  330.3 KB |
| Part2  | 13.59 ms | 0.037 ms | 0.033 ms | 93.7500 | 93.7500 | 93.7500 | 349.75 KB |

Get rid of bound checks
| Method | Mean     | Error    | StdDev   | Gen0    | Gen1    | Gen2    | Allocated |
|------- |---------:|---------:|---------:|--------:|--------:|--------:|----------:|
| Part1  | 12.63 ms | 0.090 ms | 0.079 ms | 93.7500 | 93.7500 | 93.7500 |  330.3 KB |
| Part2  | 12.65 ms | 0.051 ms | 0.046 ms | 93.7500 | 93.7500 | 93.7500 | 349.75 KB |

Use ints instead of longs
| Method | Mean     | Error    | StdDev   | Gen0    | Gen1    | Gen2    | Allocated |
|------- |---------:|---------:|---------:|--------:|--------:|--------:|----------:|
| Part1  | 11.89 ms | 0.049 ms | 0.043 ms | 46.8750 | 46.8750 | 46.8750 | 174.97 KB |
| Part2  | 12.02 ms | 0.076 ms | 0.071 ms | 46.8750 | 46.8750 | 46.8750 | 194.41 KB |

Stackalloc, cleanup, no more `visited` map in p2.
| Method | Mean     | Error    | StdDev   | Allocated |
|------- |---------:|---------:|---------:|----------:|
| Part1  | 11.98 ms | 0.029 ms | 0.024 ms |      30 B |
| Part2  | 11.97 ms | 0.107 ms | 0.100 ms |      30 B |

Only check every 2 tiles
| Method | Mean     | Error     | StdDev    | Allocated |
|------- |---------:|----------:|----------:|----------:|
| Part1  | 9.750 ms | 0.0289 ms | 0.0256 ms |      30 B |
| Part2  | 9.537 ms | 0.0557 ms | 0.0493 ms |      30 B |
 */
public class Day16 : AdventBase
{
    public override int Year => 2024;
    public override int Day => 16;

    private const int step = 2;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetCost<TLeft, TRight>()
        where TLeft : IConst<Direction>
        where TRight : IConst<Direction>
    {
        if (TLeft.Value == TRight.Value)
            return step;
        
        return (TRight.Value, TLeft.Value) switch
        {
            (Direction.Right, Direction.Up or Direction.Down) => 1000 + step,
            (Direction.Right, Direction.Left) => 2000 + step,
            (Direction.Left, Direction.Up or Direction.Down) => 1000 + step,
            (Direction.Left, Direction.Right) => 2000 + step,
            (Direction.Up, Direction.Left or Direction.Right) => 1000 + step,
            (Direction.Up, Direction.Down) => 2000 + step,
            (Direction.Down, Direction.Left or Direction.Right) => 1000 + step,
            (Direction.Down, Direction.Up) => 2000 + step,
            _ => 1,
        };
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetCost(Direction curDir, Direction targetDir)
    {
        if (curDir == targetDir)
            return step;
        
        return (targetDir, curDir) switch
        {
            (Direction.Right, Direction.Up or Direction.Down) => 1000 + step,
            (Direction.Right, Direction.Left) => 2000 + step,
            (Direction.Left, Direction.Up or Direction.Down) => 1000 + step,
            (Direction.Left, Direction.Right) => 2000 + step,
            (Direction.Up, Direction.Left or Direction.Right) => 1000 + step,
            (Direction.Up, Direction.Down) => 2000 + step,
            (Direction.Down, Direction.Left or Direction.Right) => 1000 + step,
            (Direction.Down, Direction.Up) => 2000 + step,
            _ => 1,
        };
    }

    private bool Visit2<TDir, TDirE>(Span2D<byte> map, Span2D<(int, Direction)> scores, int x, int y, int score)
        where TDir : IDirPicker
        where TDirE : IConst<Direction>
    {
        ref var storedScore = ref scores.DangerousGetReferenceAt(y, x);
        if (storedScore.Item1 <= score)
            return true;
        
        storedScore = (score, TDirE.Value);
        var anySuccess = false;
        
        if (TDir.Right && map.DangerousGetReferenceAt(y, x + 1) != '#')
            anySuccess |= Visit2<ExceptLeftPicker, DirectionRight>(map, scores, x + step, y, score + GetCost<TDirE, DirectionRight>());
        if (TDir.Left && map.DangerousGetReferenceAt(y, x - 1) != '#')
            anySuccess |= Visit2<ExceptRightPicker, DirectionLeft>(map, scores, x - step, y, score + GetCost<TDirE, DirectionLeft>());
        if (TDir.Up && map.DangerousGetReferenceAt(y - 1, x) != '#')
            anySuccess |= Visit2<ExceptDownPicker, DirectionUp>(map, scores, x , y - step, score + GetCost<TDirE, DirectionUp>());
        if (TDir.Down && map.DangerousGetReferenceAt(y + 1, x) != '#')
            anySuccess |= Visit2<ExceptUpPicker, DirectionDown>(map, scores, x , y + step, score + GetCost<TDirE, DirectionDown>());

        if (y == 1 && x == scores.Width - 2)
            return true;
        
        if (!anySuccess)
        {
            map[y, x] = (byte)'#';
            return false;
        }
        
        return anySuccess;
    }
    
    private void VisitBestPathLocations<TDir, TDirE>(Span2D<byte> map, Span2D<(int, Direction)> scores, 
        int x, int y, int score, ref int visitedCount)
        where TDir : IDirPicker
        where TDirE : IConst<Direction>
    {
        ref var storedScore = ref scores[y, x];
        var normalizedScore = (ulong)storedScore.Item1 + (ulong)GetCost(Rotate(Rotate(TDirE.Value)), storedScore.Item2) - step;
        if (normalizedScore != (ulong)score)
            return;
        if (map[y, x] == '#')
        {
            visitedCount++;
            return;
        }
        map[y, x] = (byte)'#';
        visitedCount += 2;
        
        if (TDir.Down && map[y + 1, x] != '#')
            VisitBestPathLocations<ExceptUpPicker, DirectionDown>(map, scores, x , y + step, score - GetCost<TDirE, DirectionDown>(), ref visitedCount);
        if (TDir.Right && map[y, x + 1] != '#')
            VisitBestPathLocations<ExceptLeftPicker, DirectionRight>(map, scores, x + step, y, score - GetCost<TDirE, DirectionRight>(), ref visitedCount);
        if (TDir.Left && map[y, x - 1] != '#')
            VisitBestPathLocations<ExceptRightPicker, DirectionLeft>(map, scores, x - step, y, score - GetCost<TDirE, DirectionLeft>(), ref visitedCount);
        if (TDir.Up && map[y - 1, x] != '#')
            VisitBestPathLocations<ExceptDownPicker, DirectionUp>(map, scores, x , y - step, score - GetCost<TDirE, DirectionUp>(), ref visitedCount);
    }
    
    protected override object Part1Impl()
    {
        var mapOrig = Input.Create2DMap();
        Span<byte> map1d = stackalloc byte[mapOrig.Width * mapOrig.Height];
        var map = Span2D<byte>.DangerousCreate(ref map1d[0], mapOrig.Height,mapOrig.Width, 0);
        mapOrig.CopyTo(map1d);
        
        Span<(int, Direction)> scores1d = stackalloc (int, Direction)[(map.Width-1) * map.Height];
        var scores = Span2D<(int, Direction)>.DangerousCreate(ref scores1d[0], map.Height,map.Width-1, 0);
        scores.Fill((int.MaxValue, Direction.Right));
        
        Visit2<AllDirPicker, DirectionRight>(map, scores, 1, map.Height - 2, 0);

        return scores[1, scores.Width - 2].Item1; // 127520
    }
    
    protected override object Part2Impl()
    {
        var mapOrig = Input.Create2DMap();
        Span<byte> map1d = stackalloc byte[mapOrig.Width * mapOrig.Height];
        var map = Span2D<byte>.DangerousCreate(ref map1d[0], mapOrig.Height,mapOrig.Width, 0);
        mapOrig.CopyTo(map1d);
        
        Span<(int, Direction)> scores1d = stackalloc (int, Direction)[(map.Width-1) * map.Height];
        var scores = Span2D<(int, Direction)>.DangerousCreate(ref scores1d[0], map.Height,map.Width-1, 0);
        scores.Fill((int.MaxValue, Direction.Right));
        
        Visit2<AllDirPicker, DirectionRight>(map, scores, 1, map.Height - 2, 0);

        var unique = 0;
        var dir = Rotate(Rotate(scores[1, scores.Width - 2].Item2));
        switch (dir)
        {
            case Direction.Right:
                VisitBestPathLocations<AllDirPicker, DirectionRight>(map, scores, scores.Width - 2, 1,
                    scores[1,scores.Width - 2].Item1, ref unique);
                break;
            case Direction.Left:
                VisitBestPathLocations<AllDirPicker, DirectionLeft>(map, scores, scores.Width - 2, 1,
                    scores[1,scores.Width - 2].Item1, ref unique);
                break;
            case Direction.Up:
                VisitBestPathLocations<AllDirPicker, DirectionUp>(map, scores, scores.Width - 2, 1,
                    scores[1,scores.Width - 2].Item1, ref unique);
                break;
            case Direction.Down:
                VisitBestPathLocations<AllDirPicker, DirectionDown>(map, scores, scores.Width - 2, 1,
                    scores[1,scores.Width - 2].Item1, ref unique);
                break;
        }

        return unique - 1; // 565
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Direction Rotate(Direction guardDir)
    {
        ReadOnlySpan<Direction> r = [Direction.None, Direction.Down, Direction.Up, Direction.None,
            Direction.Right, Direction.None, Direction.None, Direction.None,
            Direction.Left];
        return r.DangerousGetReferenceAt((int)guardDir);
    }
    
    private static void PrintBoard(ReadOnlySpan2D<byte> map, Span2D<(int, Direction)> scores)
    {
        for (int y = 0; y < map.Height; y++)
        {
            for (int x = 0; x < map.Width - 1; x++)
            {
                if (map[y, x] == '#')
                    Console.Write("#### ");
                else if (scores[y, x].Item1 == int.MaxValue)
                    Console.Write("---- ");
                else
                    Console.Write(scores[y, x].Item1.ToString("x4") + " ");
            }

            Console.WriteLine();
        }
    }
    
    private static void PrintBoardUnique(ReadOnlySpan2D<byte> map, Span2D<bool> scores)
    {
        for (int y = 0; y < map.Height; y++)
        {
            for (int x = 0; x < map.Width - 1; x++)
            {
                if (map[y, x] == '#')
                    Console.Write("#### ");
                else if (scores[y, x])
                    Console.Write("OOOO ");
                else
                    Console.Write("NNNN ");
            }

            Console.WriteLine();
        }
    }
}