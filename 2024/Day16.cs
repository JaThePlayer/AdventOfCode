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
 */
public class Day16 : AdventBase
{
    public override int Year => 2024;
    public override int Day => 16;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetCost<TLeft, TRight>()
        where TLeft : IConst<Direction>
        where TRight : IConst<Direction>
    {
        if (TLeft.Value == TRight.Value)
            return 1;
        
        return (TRight.Value, TLeft.Value) switch
        {
            (Direction.Right, Direction.Up or Direction.Down) => 1001,
            (Direction.Right, Direction.Left) => 2001,
            (Direction.Left, Direction.Up or Direction.Down) => 1001,
            (Direction.Left, Direction.Right) => 2001,
            (Direction.Up, Direction.Left or Direction.Right) => 1001,
            (Direction.Up, Direction.Down) => 2001,
            (Direction.Down, Direction.Left or Direction.Right) => 1001,
            (Direction.Down, Direction.Up) => 2001,
            _ => 1,
        };
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetCost(Direction curDir, Direction targetDir)
    {
        if (curDir == targetDir)
            return 1;
        
        return (targetDir, curDir) switch
        {
            (Direction.Right, Direction.Up or Direction.Down) => 1001,
            (Direction.Right, Direction.Left) => 2001,
            (Direction.Left, Direction.Up or Direction.Down) => 1001,
            (Direction.Left, Direction.Right) => 2001,
            (Direction.Up, Direction.Left or Direction.Right) => 1001,
            (Direction.Up, Direction.Down) => 2001,
            (Direction.Down, Direction.Left or Direction.Right) => 1001,
            (Direction.Down, Direction.Up) => 2001,
            _ => 1,
        };
    }
    
    private long Visit<TDir, TDirE>(ReadOnlySpan2D<byte> map, Span2D<long> scores, int x, int y, Direction dir, long score)
        where TDir : IDirPicker
        where TDirE : IConst<Direction>
    {
        ref var storedScore = ref scores[y, x];
        if (storedScore <= score)
        {
            return score;
        }
        storedScore = score;
        
        if (TDir.Right && x + 1 < map.Width && map[y, x + 1] != '#')
            Visit<ExceptLeftPicker, DirectionRight>(map, scores, x + 1, y, Direction.Right, score + GetCost<TDirE, DirectionRight>());
        if (TDir.Left && x > 1 && map[y, x - 1] != '#')
            Visit<ExceptRightPicker, DirectionLeft>(map, scores, x - 1, y, Direction.Left, score + GetCost<TDirE, DirectionLeft>());
        if (TDir.Up && y > 1 && map[y - 1, x] != '#')
            Visit<ExceptDownPicker, DirectionUp>(map, scores, x , y - 1, Direction.Up, score + GetCost<TDirE, DirectionUp>());
        if (TDir.Down && y + 1 < map.Height && map[y + 1, x] != '#')
            Visit<ExceptUpPicker, DirectionDown>(map, scores, x , y + 1, Direction.Down, score + GetCost<TDirE, DirectionDown>());
        
        return score;
    }
    
    
    protected override object Part1Impl()
    {
        var mapOrig = Input.Create2DMap();
        var map1d = new byte[mapOrig.Width * mapOrig.Height];
        var map = Span2D<byte>.DangerousCreate(ref map1d[0], mapOrig.Height,mapOrig.Width, 0);
        mapOrig.CopyTo(map1d);
        
        var scores1d = new (int, Direction)[(map.Width-1) * map.Height];
        var scores = Span2D<(int, Direction)>.DangerousCreate(ref scores1d[0], map.Height,map.Width-1, 0);
        scores.Fill((int.MaxValue, Direction.Right));
        
        var sx = 1;
        var sy = map.Height - 2;
        var dir = Direction.Right;

        Visit2<AllDirPicker, DirectionRight>(map, scores, sx, sy, 0);

        return scores[1, scores.Width - 2].Item1; // 127520
    }

    private bool Visit2<TDir, TDirE>(Span2D<byte> map, Span2D<(int, Direction)> scores, int x, int y, int score)
        where TDir : IDirPicker
        where TDirE : IConst<Direction>
    {
        ref var storedScore = ref scores.DangerousGetReferenceAt(y, x);
        //var normalizedScore = (ulong)storedScore.Item1 + (ulong)GetCost(storedScore.Item2, dir) - 1;
        if (storedScore.Item1 <= score)
        {
            return true; // return normalizedScore == (ulong)score;
        }
        storedScore = (score, TDirE.Value);
        bool anySuccess = false;
        
        if (TDir.Right && map.DangerousGetReferenceAt(y, x + 1) != '#')
            anySuccess |= Visit2<ExceptLeftPicker, DirectionRight>(map, scores, x + 1, y, score + GetCost<TDirE, DirectionRight>());
        if (TDir.Left && map.DangerousGetReferenceAt(y, x - 1) != '#')
            anySuccess |= Visit2<ExceptRightPicker, DirectionLeft>(map, scores, x - 1, y, score + GetCost<TDirE, DirectionLeft>());
        if (TDir.Up && map.DangerousGetReferenceAt(y - 1, x) != '#')
            anySuccess |= Visit2<ExceptDownPicker, DirectionUp>(map, scores, x , y - 1, score + GetCost<TDirE, DirectionUp>());
        if (TDir.Down && map.DangerousGetReferenceAt(y + 1, x) != '#')
            anySuccess |= Visit2<ExceptUpPicker, DirectionDown>(map, scores, x , y + 1, score + GetCost<TDirE, DirectionDown>());

        if (y == 1 && x == scores.Width - 2)
            return true;
        
        if (!anySuccess)
        {
            map[y, x] = (byte)'#';
            return false;
        }
        
        return anySuccess;
    }
    
    private bool Visit3<TDir>(Span2D<byte> map, Span2D<(int, Direction)> scores, Span2D<bool> visited, int x, int y, Direction dir, int score, ref int visitedCount)
        where TDir : IDirPicker
    {
        ref var v = ref visited[y, x];
        if (!v)
        {
            ref var storedScore = ref scores[y, x];
            var normalizedScore = (ulong)storedScore.Item1 + (ulong)GetCost(Rotate(Rotate(storedScore.Item2)), dir) - 1;
            
            if (normalizedScore == (ulong)score)
            {
                v = true;
                visitedCount++;
            }
            else
                return false;
        }
        else
            return false;
        
        bool anySuccess = false;
        if (TDir.Down && y + 1 < map.Height && map[y + 1, x] != '#')
            anySuccess |= Visit3<ExceptUpPicker>(map, scores, visited, x , y + 1, Direction.Down, score - GetCost(dir, Direction.Down), ref visitedCount);
        if (TDir.Right && x + 1 < map.Width && map[y, x + 1] != '#')
            anySuccess |= Visit3<ExceptLeftPicker>(map, scores, visited, x + 1, y, Direction.Right, score - GetCost(dir, Direction.Right), ref visitedCount);
        if (TDir.Left && x > 1 && map[y, x - 1] != '#')
            anySuccess |= Visit3<ExceptRightPicker>(map, scores, visited, x - 1, y, Direction.Left, score - GetCost(dir, Direction.Left), ref visitedCount);
        if (TDir.Up && y > 1 && map[y - 1, x] != '#')
            anySuccess |= Visit3<ExceptDownPicker>(map, scores, visited, x , y - 1, Direction.Up, score - GetCost(dir, Direction.Up), ref visitedCount);

        if (!anySuccess)
        {
            //visitedCount--;
            return false;
        }
        
        return anySuccess;
    }
    
    protected override object Part2Impl()
    {
        var mapOrig = Input.Create2DMap();
        var map1d = new byte[mapOrig.Width * mapOrig.Height];
        var map = Span2D<byte>.DangerousCreate(ref map1d[0], mapOrig.Height,mapOrig.Width, 0);
        mapOrig.CopyTo(map1d);
        
        var scores1d = new (int, Direction)[(map.Width-1) * map.Height];
        var scores = Span2D<(int, Direction)>.DangerousCreate(ref scores1d[0], map.Height,map.Width-1, 0);
        scores.Fill((int.MaxValue, Direction.Right));
        
        var sx = 1;
        var sy = map.Height - 2;
        var dir = Direction.Right;
        
        Visit2<AllDirPicker, DirectionRight>(map, scores, sx, sy, 0);

        //PrintBoard(map, scores);
        
        var visited1d = new bool[(map.Width-1) * map.Height];
        var visited = Span2D<bool>.DangerousCreate(ref visited1d[0], map.Height,map.Width-1, 0);

        var unique = 0;
        Visit3<AllDirPicker>(map, scores, visited, scores.Width - 2, 1, 
            Rotate(Rotate(scores[1,scores.Width - 2 ].Item2)), scores[1,scores.Width - 2 ].Item1, ref unique);

        //Console.WriteLine();
        //Console.WriteLine();
        //PrintBoardUnique(map, visited);

        return unique; // 565
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