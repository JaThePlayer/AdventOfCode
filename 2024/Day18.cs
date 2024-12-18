using CommunityToolkit.HighPerformance;

namespace AoC._2024;

/*
After part 2 rework
| Method | Mean      | Error     | StdDev    | Allocated |
|------- |----------:|----------:|----------:|----------:|
| Part1  | 15.564 ms | 0.0541 ms | 0.0506 ms |      30 B |
| Part2  |  4.604 ms | 0.0233 ms | 0.0218 ms |      39 B |
 */
public class Day18 : AdventBase
{
    private const int MapWidth = 70+1;
    private const int MapHeight = 70 + 1;
    private const int MaxBytes = 1024;
    
    public override int Year => 2024;
    public override int Day => 18;
    
    private bool VisitPart1<TDir>(Span2D<byte> map, Span2D<int> scores, int x, int y, int score)
        where TDir : IDirPicker
    {
        ref var storedScore = ref scores.DangerousGetReferenceAt(y, x);
        if (storedScore <= score)
            return true;
        
        storedScore = score;
        var anySuccess = false;
        
        if (TDir.Right && x + 1 < map.Width && map.DangerousGetReferenceAt(y, x + 1) != '#')
            anySuccess |= VisitPart1<ExceptLeftPicker>(map, scores, x + 1, y, score + 1);
        if (TDir.Left && x >= 1 && map.DangerousGetReferenceAt(y, x - 1) != '#')
            anySuccess |= VisitPart1<ExceptRightPicker>(map, scores, x - 1, y, score + 1);
        if (TDir.Up && y >= 1 && map.DangerousGetReferenceAt(y - 1, x) != '#')
            anySuccess |= VisitPart1<ExceptDownPicker>(map, scores, x , y - 1, score + 1);
        if (TDir.Down && y + 1 < map.Height && map.DangerousGetReferenceAt(y + 1, x) != '#')
            anySuccess |= VisitPart1<ExceptUpPicker>(map, scores, x , y + 1, score + 1);

        if (y == scores.Height - 1 && x == scores.Width - 1)
            return true;

        return anySuccess;
    }
    
    private bool VisitBestPathLocations<TDir>(Span2D<byte> map, Span2D<int> scores, 
        int x, int y, int score, Span2D<byte> visited)
        where TDir : IDirPicker
    {
        if (scores[y, x] != score)
            return false;
        
        ref var visitedRef = ref visited.DangerousGetReferenceAt(y, x);
        if (visitedRef > 0)
        {
            visitedRef = 1;
            return false;
        }
        visitedRef = 1;

        if (TDir.Right && x + 1 < map.Width && map[y, x + 1] != '#' && VisitBestPathLocations<ExceptLeftPicker>(map, scores, x + 1, y, score - 1, visited))
            return true;
        if (TDir.Left && x >= 1 && map[y, x - 1] != '#' && VisitBestPathLocations<ExceptRightPicker>(map, scores, x - 1, y, score - 1, visited))
            return true;
        if (TDir.Up && y >= 1 && map[y - 1, x] != '#' && VisitBestPathLocations<ExceptDownPicker>(map, scores, x , y - 1, score - 1, visited))
            return true;

        return TDir.Down && y + 1 < map.Height && map[y + 1, x] != '#' && VisitBestPathLocations<ExceptUpPicker>(map, scores, x , y + 1, score - 1, visited);
    }
    
    
    private bool VisitPart2<TDir>(Span2D<byte> map, Span2D<int> scores, int x, int y, int score)
        where TDir : IDirPicker
    {
        ref var storedScore = ref scores.DangerousGetReferenceAt(y, x);
        if (storedScore != int.MaxValue)
            return false;
        
        storedScore = score;
        
        if (y == scores.Height - 1 && x == scores.Width - 1)
            return true;

        if (TDir.Right && x + 1 < map.Width && map.DangerousGetReferenceAt(y, x + 1) != '#' && VisitPart2<ExceptLeftPicker>(map, scores, x + 1, y, score + 1))
            return true;
        if (TDir.Up && y >= 1 && map.DangerousGetReferenceAt(y - 1, x) != '#' && VisitPart2<ExceptDownPicker>(map, scores, x , y - 1, score + 1))
            return true;
        if (TDir.Down && y + 1 < map.Height && map.DangerousGetReferenceAt(y + 1, x) != '#' && VisitPart2<ExceptUpPicker>(map, scores, x , y + 1, score + 1))
            return true;
        return TDir.Left && x >= 1 && map.DangerousGetReferenceAt(y, x - 1) != '#' && VisitPart2<ExceptRightPicker>(map, scores, x - 1, y, score + 1);
    }
    
    protected override object Part1Impl()
    {
        Span<byte> map1d = stackalloc byte[MapWidth * MapHeight];
        var map = Span2D<byte>.DangerousCreate(ref map1d[0], MapHeight, MapWidth, 0);
        Span<int> scores1d = stackalloc int[MapWidth * MapHeight];
        var scores = Span2D<int>.DangerousCreate(ref scores1d[0], MapHeight, MapWidth, 0);
        scores1d.Fill(int.MaxValue);

        var i = 0;
        foreach (var (x, y) in Input.TextU8.ParseSplits((byte)'\n', 0, (span, i) =>
                 {
                     span.ParseTwoSplits((byte)',', Util.FastParseInt<int>, out var x, out var y);
                     return (x, y);
                 }))
        {
            map[y, x] = (byte)'#';
            i++;
            if (i >= MaxBytes)
                break;
        }

        VisitPart1<AllDirPicker>(map, scores, 0, 0, 0);

        return scores[^1, ^1];
    }

    protected override object Part2Impl()
    {
        Span<byte> map1d = stackalloc byte[MapWidth * MapHeight];
        var map = Span2D<byte>.DangerousCreate(ref map1d[0], MapHeight, MapWidth, 0);
        Span<int> scores1d = stackalloc int[MapWidth * MapHeight];
        var scores = Span2D<int>.DangerousCreate(ref scores1d[0], MapHeight, MapWidth, 0);
        Span<byte> visited1d = stackalloc byte[MapWidth * MapHeight];
        var visited = Span2D<byte>.DangerousCreate(ref visited1d[0], MapHeight, MapWidth, 0);
        visited.Fill(0);
        scores1d.Fill(int.MaxValue);
        VisitPart2<AllDirPicker>(map, scores, 0, 0, 0);
        VisitBestPathLocations<AllDirPicker>(map, scores, scores.Width - 1, scores.Height - 1, scores[^1, ^1], visited);

        var recalculations = 0;

        var i = 0;
        foreach (var (x, y) in Input.TextU8.ParseSplits((byte)'\n', 0, (span, i) =>
                 {
                     span.ParseTwoSplits((byte)',', Util.FastParseInt<int>, out var x, out var y);
                     return (x, y);
                 }))
        {
            i++;
            map[y, x] = (byte)'#';

            if (visited[y, x] == 0)
                continue;
            
            // rock just landed on the best path, recalculate!
            //Console.WriteLine($"RECALCULATING: {i}");
            recalculations++;
            visited.Fill(0);
            scores1d.Fill(int.MaxValue);
            if (!VisitPart2<AllDirPicker>(map, scores, 0, 0, 0))
            {
                //Console.WriteLine($"Recalc count: {recalculations}"); // 221
                return $"{x},{y}"; // 41,26
            }
            VisitBestPathLocations<AllDirPicker>(map, scores, scores.Width - 1, scores.Height - 1, scores[^1, ^1], visited);
        }

        return -1;
    }
    
    private static void PrintBoard(ReadOnlySpan2D<byte> map, Span2D<int> scores)
    {
        for (int y = 0; y < map.Height; y++)
        {
            for (int x = 0; x < map.Width; x++)
            {
                if (map[y, x] == '#')
                    Console.Write("#### ");
                else if (scores[y, x] == int.MaxValue)
                    Console.Write("---- ");
                else
                    Console.Write(scores[y, x].ToString("x4") + " ");
            }

            Console.WriteLine();
        }
    }
}