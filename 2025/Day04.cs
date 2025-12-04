using System.Runtime.CompilerServices;
using CommunityToolkit.HighPerformance;

namespace AoC._2025;

/*
Initial
| Method | Mean        | Error    | StdDev   | Gen0   | Allocated |
|------- |------------:|---------:|---------:|-------:|----------:|
| Part1  |    92.02 us | 0.574 us | 0.509 us |      - |      24 B |
| Part2  | 1,659.68 us | 4.994 us | 4.170 us | 1.9531 |   18961 B |

P2 rework: neighCounts
| Method | Mean     | Error     | StdDev    | Gen0   | Allocated |
|------- |---------:|----------:|----------:|-------:|----------:|
| Part2  | 1.020 ms | 0.0071 ms | 0.0063 ms | 3.9063 |  37.01 KB |

P2: Queue
| Method | Mean     | Error   | StdDev  | Gen0    | Gen1    | Gen2    | Allocated |
|------- |---------:|--------:|--------:|--------:|--------:|--------:|----------:|
| Part2  | 820.2 us | 1.86 us | 1.55 us | 41.0156 | 41.0156 | 41.0156 | 293.33 KB |

Less enqueue
| Method | Mean     | Error   | StdDev  | Gen0   | Gen1   | Allocated |
|------- |---------:|--------:|--------:|-------:|-------:|----------:|
| Part2  | 659.0 us | 2.64 us | 2.34 us | 7.8125 | 0.9766 |  69.25 KB |

Fill(255) to remove map check in EnqueueVisit, map is no longer mutable
| Method | Mean     | Error   | StdDev  | Gen0   | Gen1   | Allocated |
|------- |---------:|--------:|--------:|-------:|-------:|----------:|
| Part2  | 309.6 us | 1.70 us | 1.59 us | 5.8594 | 0.4883 |  50.76 KB |

AggressiveInlining on EnqueueVisit
| Method | Mean     | Error   | StdDev  | Gen0   | Gen1   | Allocated |
|------- |---------:|--------:|--------:|-------:|-------:|----------:|
| Part2  | 268.6 us | 1.98 us | 1.85 us | 5.8594 | 0.4883 |  50.76 KB |

microopt in EnqueueVisit (no more ref, just --)
| Method | Mean     | Error   | StdDev  | Gen0   | Gen1   | Allocated |
|------- |---------:|--------:|--------:|-------:|-------:|----------:|
| Part2  | 259.0 us | 2.79 us | 2.61 us | 5.8594 | 0.4883 |  50.76 KB |

DangerousGetReferenceAt in EnqueueVisit
| Method | Mean     | Error   | StdDev  | Gen0   | Gen1   | Allocated |
|------- |---------:|--------:|--------:|-------:|-------:|----------:|
| Part2  | 251.1 us | 0.64 us | 0.50 us | 5.8594 | 0.4883 |  50.76 KB |

TwoDimUtils
| Method | Mean      | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|------- |----------:|---------:|---------:|-------:|-------:|----------:|
| Part1  |  70.61 us | 1.194 us | 1.226 us |      - |      - |      24 B |
| Part2  | 211.86 us | 1.097 us | 1.026 us | 6.1035 | 0.4883 |   51976 B |
*/
public class Day04 : AdventBase
{
    public override int Year => 2025;
    public override int Day => 4;

    private ref struct NeighborCountVisitor : ITileFilter<byte>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Matches(byte tile) => tile == '@';
    }
    
    protected override object Part1Impl()
    {
        var map = Input.Create2DMap();
        var sum = 0;
        
        for (var y = 0; y < map.Height; y++)
        {
            for (var x = 0; x < map.Width - 1; x++)
            {
                var cell = map[y, x];
                if (cell != '@')
                    continue;

                if (TwoDimUtils.CountNeighborsMatching(ref map, default(NeighborCountVisitor), y, x) < 4)
                    sum++;
            }
        }

        return sum; // 1543
    }

    private readonly ref struct Part2Visitor(Queue<(int y, int x)> toVisit) : ITileVisitor<byte>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Visit(ref byte count, int y, int x)
        {
            if (--count == 3)
                toVisit.Enqueue((y, x));
        }
    }
    
    protected override object Part2Impl()
    {
        var map = Input.Create2DMap();
        var neighborCounts = Span2D<byte>.DangerousCreate(ref (new byte[map.Height * map.Width])[0], map.Height, map.Width, 0);
        neighborCounts.Fill(255);
        var sum = 0;
        Queue<(int y, int x)> toVisit = new();
        
        for (var y = 0; y < map.Height; y++)
        {
            for (var x = 0; x < map.Width - 1; x++)
            {
                var cell = map[y, x];
                if (cell != '@')
                    continue;
                
                var neigh = TwoDimUtils.CountNeighborsMatching(ref map, default(NeighborCountVisitor), y, x);
                neighborCounts[y, x] = (byte)neigh;
                
                if (neigh < 4)
                    toVisit.Enqueue((y, x));
            }
        }

        var visitor = new Part2Visitor(toVisit);
        while (toVisit.TryDequeue(out var p))
        {
            sum++;
            TwoDimUtils.ForEachNeighbor(ref neighborCounts, ref visitor, p.y, p.x);
        }

        return sum; // 9038
    }
}