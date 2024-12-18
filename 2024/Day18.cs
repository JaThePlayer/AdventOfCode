//#define TEST_INPUT

using System.Runtime.CompilerServices;
using CommunityToolkit.HighPerformance;

namespace AoC._2024;

/*
After part 2 rework
| Method | Mean      | Error     | StdDev    | Allocated |
|------- |----------:|----------:|----------:|----------:|
| Part1  | 15.564 ms | 0.0541 ms | 0.0506 ms |      30 B |
| Part2  |  4.604 ms | 0.0233 ms | 0.0218 ms |      39 B |

Dijkstra's algorithm
| Method | Mean       | Error   | StdDev  | Gen0   | Allocated |
|------- |-----------:|--------:|--------:|-------:|----------:|
| Part1  |   197.3 us | 1.16 us | 1.03 us | 0.9766 |    8368 B |
| Part2  | 4,821.2 us | 3.77 us | 2.94 us |      - |      39 B |

Rework enqueueing in p1
| Method | Mean       | Error    | StdDev   | Gen0   | Allocated |
|------- |-----------:|---------:|---------:|-------:|----------:|
| Part1  |   150.0 us |  0.40 us |  0.36 us | 0.9766 |    8368 B |
| Part2  | 4,783.5 us | 36.66 us | 34.29 us |      - |      39 B |

P2: Binary search
| Method | Mean     | Error   | StdDev  | Gen0   | Allocated |
|------- |---------:|--------:|--------:|-------:|----------:|
| Part1  | 149.0 us | 0.54 us | 0.42 us | 0.9766 |    8368 B |
| Part2  | 109.1 us | 0.31 us | 0.26 us |      - |      32 B |

P1: no more stackalloc, use shorts
| Method | Mean      | Error    | StdDev   | Gen0   | Allocated |
|------- |----------:|---------:|---------:|-------:|----------:|
| Part1  | 124.95 us | 1.410 us | 1.250 us | 2.6855 |   23552 B |
| Part2  |  99.48 us | 0.462 us | 0.385 us |      - |      32 B |

P1: no more map
| Method | Mean      | Error    | StdDev   | Gen0   | Allocated |
|------- |----------:|---------:|---------:|-------:|----------:|
| Part1  | 103.20 us | 1.989 us | 2.915 us | 2.1973 |   18480 B |
| Part2  |  99.65 us | 0.358 us | 0.335 us |      - |      32 B |
 */
public class Day18 : AdventBase
{
    #if TEST_INPUT
    private const int MapWidth = 6+1;
    private const int MapHeight = 6 + 1;
    private const int MaxBytes = 12;
    #else
    private const int MapWidth = 70+1;
    private const int MapHeight = 70 + 1;
    private const int MaxBytes = 1024;
    #endif
    
    public override int Year => 2024;
    public override int Day => 18;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static (int x, int y) ParseLine(ReadOnlySpan<byte> span)
    {
        if (span.Length == 5)
            return (Util.FastParse2DigitInt<int>(span), Util.FastParse2DigitInt<int>(span, 3));

        span.ParseTwoSplits((byte)',', Util.FastParseInt<int>, out var x, out var y);
        return (x, y);
    }
    
    protected override object Part1Impl()
    {
        Span<short> scores1d = new short[MapWidth * MapHeight];
        scores1d.Fill(short.MaxValue);
        var scores = Span2D<short>.DangerousCreate(ref scores1d[0], MapHeight, MapWidth, 0);
        
        var i = MaxBytes;
        foreach (var (x, y) in Input.TextU8.ParseSplits((byte)'\n', 0, (span, _) => ParseLine(span)))
        {
            scores.DangerousGetReferenceAt(y, x) = short.MinValue;
            i--;
            if (i <= 0)
                break;
        }
        
        PriorityQueue<(int x, int y), int> queue = new();
        scores[0, 0] = 0;
        queue.Enqueue((0, 0), 0);

        while (queue.Count > 0)
        {
            var (x, y) = queue.Dequeue();
            ref var score = ref scores.DangerousGetReferenceAt(y, x);
            
            if (y == MapHeight - 1 && x == MapWidth - 1)
                return score; // 264
            var prevScore = score;
            score = short.MinValue;
            
            if (x + 1 < MapWidth)
                Check(scores, prevScore, x + 1, y);
            if (x >= 1)
                Check(scores, prevScore, x - 1, y);
            if (y >= 1)
                Check(scores, prevScore, x, y-1);
            if (y + 1 < MapHeight)
                Check(scores, prevScore, x, y+1);
        }

        return -1;

        void Check(Span2D<short> scores, short score, int x2, int y2)
        {
            ref var newScore = ref scores.DangerousGetReferenceAt(y2, x2);
            var alt = score + 1;
            if (alt >= newScore)
                return;
            
            newScore = (short)alt;
            queue.Enqueue((x2, y2), newScore);
        }
    }
    
    private bool CanSolvePart2<TDir>(Span2D<byte> map, Span2D<byte> visited, int x, int y)
        where TDir : IDirPicker
    {
        ref var visit = ref visited.DangerousGetReferenceAt(y, x);
        if (visit != 0)
            return false;
        visit = 1;
        
        if (y == visited.Height - 1 && x == visited.Width - 1)
            return true;

        if (TDir.Right && x + 1 < map.Width && map.DangerousGetReferenceAt(y, x + 1) != '#' && CanSolvePart2<ExceptLeftPicker>(map, visited, x + 1, y))
            return true;
        if (TDir.Down && y + 1 < map.Height && map.DangerousGetReferenceAt(y + 1, x) != '#' && CanSolvePart2<ExceptUpPicker>(map, visited, x , y + 1))
            return true;
        if (TDir.Up && y >= 1 && map.DangerousGetReferenceAt(y - 1, x) != '#' && CanSolvePart2<ExceptDownPicker>(map, visited, x , y - 1))
            return true;
        return TDir.Left && x >= 1 && map.DangerousGetReferenceAt(y, x - 1) != '#' && CanSolvePart2<ExceptRightPicker>(map, visited, x - 1, y);
    }
    
    protected override object Part2Impl()
    {
        Span<byte> map1d = stackalloc byte[MapWidth * MapHeight];
        Span<byte> visited1d = stackalloc byte[MapWidth * MapHeight];
        Span<(int x, int y)> numbersBuffer = stackalloc (int x, int y)[4192];
        var map = Span2D<byte>.DangerousCreate(ref map1d[0], MapHeight, MapWidth, 0);
        var visited = Span2D<byte>.DangerousCreate(ref visited1d[0], MapHeight, MapWidth, 0);
        
        var numbers = ParseAllNumbers(numbersBuffer);

        var startI = 0;
        var endI = numbers.Length;
        
        var middle2 = endI / 2;
        for (int i = startI; i <= middle2; i++)
        {
            var (x, y) = numbers[i];
            map.DangerousGetReferenceAt(y, x) = (byte)'#';
        }
        
        while (true)
        {
            var middle = ((endI - startI) / 2)+startI;
            
            visited1d.Fill(0);
            if (CanSolvePart2<AllDirPicker>(map, visited, 0, 0))
            {
                // The solution is in the latter half
                startI = middle+1;
                
                var newMiddle = ((endI - startI) / 2)+startI;
                for (int i = startI; i <= newMiddle; i++)
                {
                    var (x, y) = numbers.DangerousGetReferenceAt(i);
                    map.DangerousGetReferenceAt(y, x) = (byte)'#';
                }
            }
            else
            {
                // Impossible to solve the maze
                if (startI == endI)
                {
                    var (x, y) = numbers[startI];
                    return $"{x},{y}";
                }
                // We overshot already - undo half of the inputs
                endI = middle;
                var newMiddle = ((endI - startI) / 2)+startI;
                for (int i = newMiddle+1; i <= middle; i++)
                {
                    var (x, y) = numbers.DangerousGetReferenceAt(i);
                    map.DangerousGetReferenceAt(y, x) = 0;
                }
            }
        }
        
        // Extracted to separate method to help the jit, saves 30us for me
        Span<(int x, int y)> ParseAllNumbers(Span<(int x, int y)> numbersBuffer)
        {
            return Input.TextU8
                .SplitSlim((byte)'\n')
                .Select(span => ParseLine(span))
                .FillSpan(numbersBuffer);
        }
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