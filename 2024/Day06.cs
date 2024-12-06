using System.Runtime.CompilerServices;
using CommunityToolkit.HighPerformance;

namespace AoC._2024;

/*
Initial (Absolutely horrible naive loop)
| Method | Mean          | Error        | StdDev       | Median        | Gen0   | Allocated |
|------- |--------------:|-------------:|-------------:|--------------:|-------:|----------:|
| Part1  |      13.65 us |     0.272 us |     0.353 us |      13.62 us | 2.0294 |   16.7 KB |
| Part2  | 205,896.78 us | 4,081.701 us | 7,565.695 us | 209,969.70 us |      - |  66.72 KB |

Part2: Only place obstacles on the path
| Method | Mean         | Error      | StdDev     | Gen0   | Allocated |
|------- |-------------:|-----------:|-----------:|-------:|----------:|
| Part1  |     16.45 us |   0.273 us |   0.242 us | 2.0142 |   16.7 KB |
| Part2  | 51,499.79 us | 298.182 us | 264.330 us |      - |   83.3 KB |

Use :byte for the enum
| Method | Mean         | Error      | StdDev    | Gen0   | Allocated |
|------- |-------------:|-----------:|----------:|-------:|----------:|
| Part1  |     15.96 us |   0.300 us |  0.334 us | 2.0294 |   16.7 KB |
| Part2  | 61,113.45 us | 102.942 us | 85.961 us |      - |   33.4 KB |

Don't collect points into a temp list
| Method | Mean         | Error      | StdDev     | Gen0   | Allocated |
|------- |-------------:|-----------:|-----------:|-------:|----------:|
| Part1  |     16.77 us |   0.316 us |   0.296 us | 2.0142 |   16.7 KB |
| Part2  | 45,285.11 us | 429.529 us | 380.766 us |      - |   33.4 KB |

Part 2: Start searching from the tile right before the new obstacle
| Method | Mean        | Error     | StdDev    | Gen0   | Allocated |
|------- |------------:|----------:|----------:|-------:|----------:|
| Part1  |    16.14 us |  0.319 us |  0.354 us | 2.0142 |   16.7 KB |
| Part2  | 9,587.69 us | 31.110 us | 25.979 us |      - |  33.37 KB |

Extract to Simulate<T>
| Method | Mean         | Error     | StdDev    | Gen0   | Allocated |
|------- |-------------:|----------:|----------:|-------:|----------:|
| Part1  |     13.18 us |  0.145 us |  0.128 us | 2.0294 |   16.7 KB |
| Part2  | 12,155.76 us | 61.186 us | 57.233 us |      - |  33.37 KB |

AggressiveInlining
| Method | Mean         | Error     | StdDev    | Gen0   | Allocated |
|------- |-------------:|----------:|----------:|-------:|----------:|
| Part1  |     13.76 us |  0.100 us |  0.088 us | 2.0294 |   16.7 KB |
| Part2  | 11,863.79 us | 45.033 us | 42.124 us |      - |  33.37 KB |

Use the Direction enum for guard dir
| Method | Mean        | Error     | StdDev    | Gen0   | Allocated |
|------- |------------:|----------:|----------:|-------:|----------:|
| Part1  |    13.71 us |  0.120 us |  0.113 us | 2.0294 |   16.7 KB |
| Part2  | 9,090.79 us | 47.247 us | 44.195 us |      - |  33.37 KB |

Lookup table for Rotate
| Method | Mean        | Error     | StdDev    | Gen0   | Allocated |
|------- |------------:|----------:|----------:|-------:|----------:|
| Part1  |    13.76 us |  0.065 us |  0.061 us | 2.0294 |   16.7 KB |
| Part2  | 8,867.86 us | 21.920 us | 17.114 us |      - |  33.37 KB |

MoveNext improvement and use it less
| Method | Mean        | Error      | StdDev    | Gen0   | Allocated |
|------- |------------:|-----------:|----------:|-------:|----------:|
| Part1  |    12.25 us |   0.217 us |  0.203 us | 2.0294 |   16.7 KB |
| Part2  | 8,377.29 us | 102.200 us | 95.598 us |      - |  33.37 KB |

Stackalloc the buffers
| Method | Mean        | Error     | StdDev    | Allocated |
|------- |------------:|----------:|----------:|----------:|
| Part1  |    10.45 us |  0.069 us |  0.064 us |      24 B |
| Part2  | 7,947.52 us | 47.033 us | 43.995 us |      26 B |
 */
public class Day06 : AdventBase
{
    public override int Year => 2024;
    public override int Day => 6;

    private enum SimulationResult
    {
        Loop,
        Exit,
    }
    
    [Flags]
    private enum Direction : byte
    {
        None = 0,
        Right = 1,
        Left = 2,
        Up = 4,
        Down = 8,
    }
    
    private interface IChecker<T>
    {
        public bool HadVisited(ref T visited, Direction dir);

        public bool ShouldEndAtLoop();
        
        public void MarkVisited(ref T visited, Direction dir);

        public void OnNewVisit(ref T visited, Direction dir, int gx, int gy, Direction ogDir, int ogx, int ogy);
    }

    private static SimulationResult Simulate<T, TChecker>(Span2D<T> visited, ref TChecker checker, ReadOnlySpan2D<char> span, 
        int sgy, int sgx, Direction guardDir)
        where TChecker : IChecker<T>, allows ref struct
    {
        var (gx, gy) = (sgx, sgy);
        var (ogx, ogy, ogDir) = (gx, gy, guardDir);

        var (dirX, dirY) = GetMovementVector(guardDir);
        
        while (true)
        {
            ref var tileVisit = ref visited.DangerousGetReferenceAt(gy, gx);
            if (checker.HadVisited(ref tileVisit, guardDir))
            {
                if (checker.ShouldEndAtLoop())
                    return SimulationResult.Loop;
            }
            else
            {
                checker.OnNewVisit(ref tileVisit, guardDir, gx, gy, ogDir, ogx, ogy);
            }
            checker.MarkVisited(ref tileVisit, guardDir);
            (ogx, ogy, ogDir) = (gx, gy, guardDir);

            var (ngx, ngy) = (gx+dirX, gy+dirY);

            // OOB check
            if ((uint)ngx >= span.Width || (uint)ngy >= span.Height)
                return SimulationResult.Exit;

            if (span.DangerousGetReferenceAt(ngy, ngx) != '#')
            {
                // valid move
                (gx, gy) = (ngx, ngy);
                continue;
            }

            // obstacle, rotate by 90
            guardDir = Rotate(guardDir);
            (dirX, dirY) = GetMovementVector(guardDir);
        }
    }

    private struct P1Checker : IChecker<bool>
    {
        public int Count;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HadVisited(ref bool visited, Direction dir)
        {
            return visited;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ShouldEndAtLoop()
        {
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MarkVisited(ref bool visited, Direction dir)
        {
            visited = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnNewVisit(ref bool visited, Direction dir, int gx, int gy, Direction ogDir, int ogx, int ogy)
        {
            Count++;
        }
    }

    private ref struct P2CheckerInitial(ReadOnlySpan2D<char> span, Span2D<Direction> visitedMap, Span2D<Direction> tempVisitedMap) : IChecker<Direction>
    {
        private readonly ReadOnlySpan2D<char> _span = span;
        private readonly Span2D<Direction> _visitedMap = visitedMap;
        private readonly Span2D<Direction> _tempVisitedMap = tempVisitedMap;

        public int Count;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HadVisited(ref Direction visited, Direction dir)
        {
            return visited != Direction.None;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ShouldEndAtLoop()
        {
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MarkVisited(ref Direction visited, Direction dir)
        {
            visited |= dir;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnNewVisit(ref Direction visited, Direction dir, int gx, int gy, Direction ogDir, int ogx, int ogy)
        {
            // See what happens if we place an obstacle at this spot.
            // Start the simulation at the current pos
            _visitedMap.CopyTo(_tempVisitedMap);
            _tempVisitedMap.DangerousGetReferenceAt(ogy, ogx) = Direction.None;
            
            var checker = new P2CheckerInner();

            ref var obstacleRef = ref _span.DangerousGetReferenceAt(gy, gx);
            var prevTile = obstacleRef;
            obstacleRef = '#';
            var isLoop = Simulate(_tempVisitedMap, ref checker, _span, ogy, ogx, ogDir);
            obstacleRef = prevTile;
            Count += isLoop == SimulationResult.Loop ? 1 : 0;
        }
    }
    
    private ref struct P2CheckerInner : IChecker<Direction>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HadVisited(ref Direction visited, Direction dir)
        {
            return (visited & dir) == dir;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ShouldEndAtLoop()
        {
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MarkVisited(ref Direction visited, Direction dir)
        {
            visited |= dir;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnNewVisit(ref Direction visited, Direction dir, int gx, int gy, Direction ogDir, int ogx, int ogy)
        {

        }
    }
    
    protected override object Part1Impl()
    {
        var input = Input.Text.AsSpan();
        var lineWidth = input.IndexOf('\n') + 1;
        var guardIdx = input.IndexOf('^');
        var span = ReadOnlySpan2D<char>.DangerousCreate(input[0], input.Length / lineWidth + 1, lineWidth, 0);
        Span<bool> visited1D = stackalloc bool[span.Height * span.Width];
        var visited = Span2D<bool>.DangerousCreate(ref visited1D[0], span.Height, span.Width, 0);
        
        var (sgx, sgy) = (guardIdx % span.Width, guardIdx / span.Width);

        var checker = new P1Checker();

        Simulate(visited, ref checker, span, sgy, sgx, Direction.Up);
        
        return checker.Count; // 5212
    }
    
    protected override unsafe object Part2Impl()
    {
        var input = Input.Text.AsSpan();
        var lineWidth = input.IndexOf('\n') + 1;
        var guardIdx = input.IndexOf('^');
        
        var span = ReadOnlySpan2D<char>.DangerousCreate(input[0], input.Length / lineWidth + 1, lineWidth, 0);
        
        Span<Direction> visited1D = stackalloc Direction[span.Height * span.Width];
        Span<Direction> visitedTemp1D = stackalloc Direction[span.Height * span.Width];
        var visited = Span2D<Direction>.DangerousCreate(ref visited1D[0], span.Height, span.Width, 0);
        var visitedTemp = Span2D<Direction>.DangerousCreate(ref visitedTemp1D[0], span.Height, span.Width, 0);

        var (sgx, sgy) = (guardIdx % span.Width, guardIdx / span.Width);
        
        var checker = new P2CheckerInitial(span, visited, visitedTemp);

        Simulate(visited, ref checker, span, sgy, sgx, Direction.Up);

        return checker.Count; // 1767
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Direction Rotate(Direction guardDir)
        => Rotations.DangerousGetReferenceAt((int)guardDir);
    
    private static ReadOnlySpan<Direction> Rotations => [Direction.None, Direction.Down, Direction.Up, Direction.None,
        Direction.Right, Direction.None, Direction.None, Direction.None,
        Direction.Left];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static (int, int) GetMovementVector(Direction guardDir)
    {
        var movement = Movements.DangerousGetReferenceAt((int)guardDir);
        if (guardDir <= Direction.Left)
            return (movement, 0);
        return (0, movement);
    }
    
    private static ReadOnlySpan<sbyte> Movements => [0, 1, -1, 0, -1, 0, 0, 0, 1];
}