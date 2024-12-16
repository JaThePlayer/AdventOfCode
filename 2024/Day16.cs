using System.Runtime.CompilerServices;
using CommunityToolkit.HighPerformance;

namespace AoC._2024;

/*
Initial
| Method | Mean     | Error    | StdDev   | Gen0    | Gen1    | Gen2    | Allocated |
|------- |---------:|---------:|---------:|--------:|--------:|--------:|----------:|
| Part1  | 15.51 ms | 0.101 ms | 0.079 ms | 31.2500 | 31.2500 | 31.2500 | 155.39 KB |
| Part2  | 14.51 ms | 0.064 ms | 0.057 ms | 93.7500 | 93.7500 | 93.7500 | 349.74 KB |
 */
public class Day16 : AdventBase
{
    public override int Year => 2024;
    public override int Day => 16;

    private long GetCost(Direction curDir, Direction targetDir)
    {
        if (curDir == targetDir)
            return 1;
        
        return (curDir, targetDir) switch
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
    
    private long Visit<TDir>(ReadOnlySpan2D<byte> map, Span2D<long> scores, int x, int y, Direction dir, long score)
        where TDir : IDirPicker
    {
        ref var storedScore = ref scores[y, x];
        if (storedScore <= score)
        {
            return score;
        }
        storedScore = score;
        
        if (TDir.Right && x + 1 < map.Width && map[y, x + 1] != '#')
            Visit<ExceptLeftPicker>(map, scores, x + 1, y, Direction.Right, score + GetCost(dir, Direction.Right));
        if (TDir.Left && x > 1 && map[y, x - 1] != '#')
            Visit<ExceptRightPicker>(map, scores, x - 1, y, Direction.Left, score + GetCost(dir, Direction.Left));
        if (TDir.Up && y > 1 && map[y - 1, x] != '#')
            Visit<ExceptDownPicker>(map, scores, x , y - 1, Direction.Up, score + GetCost(dir, Direction.Up));
        if (TDir.Down && y + 1 < map.Height && map[y + 1, x] != '#')
            Visit<ExceptUpPicker>(map, scores, x , y + 1, Direction.Down, score + GetCost(dir, Direction.Down));
        
        return score;
    }
    
    
    protected override object Part1Impl()
    {
        var map = Input.Create2DMap();
        var visited1d = new long[(map.Width-1) * map.Height];
        var scores = Span2D<long>.DangerousCreate(ref visited1d[0], map.Height,map.Width-1, 0);
        scores.Fill(long.MaxValue);
        
        var sx = 1;
        var sy = map.Height - 2;
        var dir = Direction.Right;

        Visit<AllDirPicker>(map, scores, sx, sy, dir, 0);

        return scores[1, scores.Width - 2]; // 127520
    }

    private bool Visit2<TDir>(Span2D<byte> map, Span2D<(long, Direction)> scores, int x, int y, Direction dir, long score)
        where TDir : IDirPicker
    {
        ref var storedScore = ref scores[y, x];
        //var normalizedScore = (ulong)storedScore.Item1 + (ulong)GetCost(storedScore.Item2, dir) - 1;
        if (storedScore.Item1 <= score)
        {
            return true; // return normalizedScore == (ulong)score;
        }
        storedScore = (score, dir);
        bool anySuccess = false;
        
        if (TDir.Right && x + 1 < map.Width && map[y, x + 1] != '#')
            anySuccess |= Visit2<ExceptLeftPicker>(map, scores, x + 1, y, Direction.Right, score + GetCost(dir, Direction.Right));
        if (TDir.Left && x > 1 && map[y, x - 1] != '#')
            anySuccess |= Visit2<ExceptRightPicker>(map, scores, x - 1, y, Direction.Left, score + GetCost(dir, Direction.Left));
        if (TDir.Up && y > 1 && map[y - 1, x] != '#')
            anySuccess |= Visit2<ExceptDownPicker>(map, scores, x , y - 1, Direction.Up, score + GetCost(dir, Direction.Up));
        if (TDir.Down && y + 1 < map.Height && map[y + 1, x] != '#')
            anySuccess |= Visit2<ExceptUpPicker>(map, scores, x , y + 1, Direction.Down, score + GetCost(dir, Direction.Down));

        if (y == 1 && x == scores.Width - 2)
            return true;
        
        if (!anySuccess)
        {
            map[y, x] = (byte)'#';
            return false;
        }
        
        return anySuccess;
    }
    
    private bool Visit3<TDir>(Span2D<byte> map, Span2D<(long, Direction)> scores, Span2D<bool> visited, int x, int y, Direction dir, long score, ref int visitedCount)
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
        {
            anySuccess |= Visit3<ExceptLeftPicker>(map, scores, visited, x + 1, y, Direction.Right, score - GetCost(dir, Direction.Right), ref visitedCount);
        }
           
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
        
        var scores1d = new (long, Direction)[(map.Width-1) * map.Height];
        var scores = Span2D<(long, Direction)>.DangerousCreate(ref scores1d[0], map.Height,map.Width-1, 0);
        scores.Fill((long.MaxValue, Direction.Right));
        
        var sx = 1;
        var sy = map.Height - 2;
        var dir = Direction.Right;
        
        Visit2<AllDirPicker>(map, scores, sx, sy, dir, 0);

        //PrintBoard(map, scores);
        
        var visited1d = new bool[(map.Width-1) * map.Height];
        var visited = Span2D<bool>.DangerousCreate(ref visited1d[0], map.Height,map.Width-1, 0);

        var unique = 0;
        Visit3<AllDirPicker>(map, scores, visited, scores.Width - 2, 1, 
            Rotate(Rotate(scores[1,scores.Width - 2 ].Item2)), scores[1,scores.Width - 2 ].Item1, ref unique);

        //Console.WriteLine();
        //Console.WriteLine();
        //PrintBoardUnique(map, visited);

        return unique;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Direction Rotate(Direction guardDir)
    {
        ReadOnlySpan<Direction> r = [Direction.None, Direction.Down, Direction.Up, Direction.None,
            Direction.Right, Direction.None, Direction.None, Direction.None,
            Direction.Left];
        return r.DangerousGetReferenceAt((int)guardDir);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static (int, int) GetMovementVector(Direction guardDir)
    {
        ReadOnlySpan<long> r = [0, 1, 4294967295, 0, -4294967296, 0, 0, 0, 4294967296];
        return Unsafe.As<long, (int, int)>(ref r.DangerousGetReferenceAt((int)guardDir));
    }
    
    private static void PrintBoard(ReadOnlySpan2D<byte> map, Span2D<(long, Direction)> scores)
    {
        for (int y = 0; y < map.Height; y++)
        {
            for (int x = 0; x < map.Width - 1; x++)
            {
                if (map[y, x] == '#')
                    Console.Write("#### ");
                else if (scores[y, x].Item1 == long.MaxValue)
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