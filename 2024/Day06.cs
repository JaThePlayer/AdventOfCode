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

Fastpath for xDir>0
| Method | Mean         | Error      | StdDev     | Allocated |
|------- |-------------:|-----------:|-----------:|----------:|
| Part1  |     8.140 us |  0.0610 us |  0.0541 us |      24 B |
| Part2  | 7,142.807 us | 36.5392 us | 32.3910 us |      27 B |

Fastpath for xDir!=0
| Method | Mean         | Error      | StdDev     | Allocated |
|------- |-------------:|-----------:|-----------:|----------:|
| Part1  |     7.552 us |  0.0141 us |  0.0125 us |      24 B |
| Part2  | 6,549.366 us | 18.0962 us | 16.0418 us |      27 B |

Fast path for yDir!=0 - everything is a fast path now
| Method | Mean         | Error      | StdDev     | Allocated |
|------- |-------------:|-----------:|-----------:|----------:|
| Part1  |     6.659 us |  0.0402 us |  0.0336 us |      24 B |
| Part2  | 5,931.789 us | 26.5665 us | 24.8504 us |      27 B |

Part 2: in the inner Simulate function, only update and check the visited set when hitting a wall, not every step
(Thanks to woofdoggo on celestecord for the idea)
| Method | Mean         | Error      | StdDev     | Allocated |
|------- |-------------:|-----------:|-----------:|----------:|
| Part1  |     6.811 us |  0.0595 us |  0.0556 us |      24 B |
| Part2  | 4,261.634 us | 39.3663 us | 36.8233 us |      27 B |

NeedsToVisitEachLocation
| Method | Mean         | Error      | StdDev     | Allocated |
|------- |-------------:|-----------:|-----------:|----------:|
| Part1  |     6.689 us |  0.0578 us |  0.0541 us |      24 B |
| Part2  | 2,956.064 us | 13.8153 us | 12.9228 us |      24 B |

Transposed map + small microopts
| Method | Mean         | Error     | StdDev    | Allocated |
|------- |-------------:|----------:|----------:|----------:|
| Part1  |     6.453 us | 0.0169 us | 0.0141 us |      24 B |
| Part2  | 2,573.377 us | 6.0574 us | 5.3697 us |      24 B |
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
    
    private interface IChecker<T>
    {
        public bool HadVisited(ref T visited);
        
        public bool NeedsToVisitEachLocation();
        
        public void MarkVisited(ref T visited, Direction dir);

        public void OnNewVisit(int gx, int gy, Direction dir);

        public bool LoopsOnRotation(ref T visited, Direction dir);
    }

    private static SimulationResult Simulate<T, TChecker>(Span2D<T> visited, ref TChecker checker, 
        ReadOnlySpan2D<char> span, ReadOnlySpan2D<char> spanT, 
        int sgy, int sgx, Direction guardDir)
        where TChecker : IChecker<T>, allows ref struct
    {
        var (gx, gy) = (sgx, sgy);
        var (dirX, dirY) = GetMovementVector(guardDir);
        
        while (true)
        {
            if (dirX != 0)
            {
                // Horizontal movement - use IndexOf to benefit from SIMD
                int targetX;
                if (dirX > 0)
                {
                    // going right
                    var curRow = span.GetRowSpan(gy)[gx..];
                    targetX = curRow.IndexOf('#');
                    if (targetX == -1)
                        targetX = curRow.Length;
                    targetX += gx;
                }
                else
                {
                    // left
                    targetX = span.GetRowSpan(gy)[..gx].LastIndexOf('#');
                }
                var visitedRow = visited.GetRowSpan(gy);
                if (checker.NeedsToVisitEachLocation())
                {
                    while (gx != targetX)
                    {
                        VisitTile(ref checker, ref visitedRow.DangerousGetReferenceAt(gx));
                        gx += dirX;
                    }
                }
                else
                {
                    gx = targetX;
                }

                unchecked
                {
                    if ((uint)gx >= span.Width)
                        return SimulationResult.Exit;
                }
                
                // obstacle, rotate by 90
                gx += -dirX;
                if (checker.LoopsOnRotation(ref visitedRow.DangerousGetReferenceAt(gx), guardDir))
                    return SimulationResult.Loop;
                guardDir = Rotate(guardDir);
                (dirX, dirY) = GetMovementVector(guardDir);
                continue;
            }
            
            // vertical
            if (spanT == default)
            {
                // In part 1, transposing the map is way too costly to be worth it, so do a scalar loop instead
                while (true)
                {
                    if (checker.NeedsToVisitEachLocation())
                        VisitTile(ref checker, ref visited.DangerousGetReferenceAt(gy, gx));
                    gy += dirY;
                    unchecked
                    {
                        if ((uint)gy >= span.Height)
                            return SimulationResult.Exit;
                    }

                    if (span.DangerousGetReferenceAt(gy, gx) != '#')
                        continue;
                    // obstacle, rotate by 90
                    gy -= dirY;
                    if (checker.LoopsOnRotation(ref visited.DangerousGetReferenceAt(gy,gx), guardDir))
                        return SimulationResult.Loop;
                    guardDir = Rotate(guardDir);
                    (dirX, dirY) = GetMovementVector(guardDir);
                    break;
                }
            }
            else
            {
                // We have a transposed map, we can SIMD
                int targetY;
                if (dirY > 0)
                {
                    var curRow = spanT.GetRowSpan(gx)[gy..];
                    targetY = curRow.IndexOf('#');
                    if (targetY == -1)
                        targetY = curRow.Length;
                    targetY += gy;
                }
                else
                {
                    targetY = spanT.GetRowSpan(gx)[..gy].LastIndexOf('#');
                }
            
                if (checker.NeedsToVisitEachLocation())
                {
                    while (gy != targetY)
                    {
                        VisitTile(ref checker, ref visited.DangerousGetReferenceAt(gy, gx));
                        gy += dirY;
                    }
                }
                else
                {
                    gy = targetY;
                }

                unchecked
                {
                    if ((uint)gy >= span.Height)
                        return SimulationResult.Exit;
                }

                // obstacle, rotate by 90
                gy -= dirY;
                if (checker.LoopsOnRotation(ref visited.DangerousGetReferenceAt(gy,gx), guardDir))
                    return SimulationResult.Loop;
                guardDir = Rotate(guardDir);
                (dirX, dirY) = GetMovementVector(guardDir);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void VisitTile(ref TChecker checker, ref T tileVisit)
        {
            if (!checker.HadVisited(ref tileVisit))
            {
                checker.OnNewVisit(gx, gy, guardDir);
            }
            
            checker.MarkVisited(ref tileVisit, guardDir);
        }
    }

    private struct P1Checker : IChecker<bool>
    {
        public int Count;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HadVisited(ref bool visited)
        {
            return visited;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool NeedsToVisitEachLocation() => true;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MarkVisited(ref bool visited, Direction dir)
        {
            visited = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnNewVisit(int gx, int gy, Direction dir)
        {
            Count++;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool LoopsOnRotation(ref bool visited, Direction dir)
        {
            return false;
        }
    }

    private ref struct P2CheckerInitial(ReadOnlySpan2D<char> span, Span2D<char> spanT, Span2D<Direction> visitedMap, Span2D<Direction> tempVisitedMap) 
        : IChecker<Direction>
    {
        private readonly ReadOnlySpan2D<char> _span = span;
        private readonly ReadOnlySpan2D<char> _spanT = spanT;
        private readonly Span2D<Direction> _visitedMap = visitedMap;
        private readonly Span2D<Direction> _tempVisitedMap = tempVisitedMap;

        public int Count;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HadVisited(ref Direction visited)
        {
            return visited != Direction.None;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool NeedsToVisitEachLocation() => true;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MarkVisited(ref Direction visited, Direction dir)
        {
            visited |= dir;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnNewVisit(int gx, int gy, Direction dir)
        {
            var (ogx, ogy) = GetMovementVector(dir);
            ogx = gx - ogx;
            ogy = gy - ogy;
            // See what happens if we place an obstacle at this spot.
            // Start the simulation at the current pos
            _visitedMap.CopyTo(_tempVisitedMap);
            _tempVisitedMap.DangerousGetReferenceAt(ogy, ogx) |= dir;
            
            var checker = new P2CheckerInner();

            ref var obstacleRef = ref _span.DangerousGetReferenceAt(gy, gx);
            ref var obstacleTRef = ref _spanT.DangerousGetReferenceAt(gx, gy);
            var prevTile = obstacleRef;
            obstacleRef = '#';
            obstacleTRef = '#';
            var isLoop = Simulate(_tempVisitedMap, ref checker, _span,_spanT, ogy, ogx, Rotate(dir));
            obstacleRef = prevTile;
            obstacleTRef = prevTile;
            Count += isLoop == SimulationResult.Loop ? 1 : 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool LoopsOnRotation(ref Direction visited, Direction dir)
        {
            return false;
        }
    }
    
    private ref struct P2CheckerInner : IChecker<Direction>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HadVisited(ref Direction visited)
        {
           return false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool NeedsToVisitEachLocation() => false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MarkVisited(ref Direction visited, Direction dir)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnNewVisit(int gx, int gy, Direction dir)
        {

        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool LoopsOnRotation(ref Direction visited, Direction dir)
        {
            if ((visited & dir) == dir)
                return true;
            visited |= dir;
            return false;
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

        Simulate(visited, ref checker, span, spanT: default, sgy, sgx, Direction.Up);
        
        return checker.Count; // 5212
    }
    
    protected override object Part2Impl()
    {
        var input = Input.Text.AsSpan();
        var lineWidth = input.IndexOf('\n') + 1;
        var guardIdx = input.IndexOf('^');
        
        var span = ReadOnlySpan2D<char>.DangerousCreate(input[0], input.Length / lineWidth + 1, lineWidth, 0);
        Span<char> spanT1D = stackalloc char[input.Length];
        var spanT = Span2D<char>.DangerousCreate(ref spanT1D[0], lineWidth, input.Length / lineWidth + 1, 0);
        span.Transpose(spanT);
        
        Span<Direction> visited1D = stackalloc Direction[span.Height * span.Width];
        Span<Direction> visitedTemp1D = stackalloc Direction[span.Height * span.Width];
        var visited = Span2D<Direction>.DangerousCreate(ref visited1D[0], span.Height, span.Width, 0);
        var visitedTemp = Span2D<Direction>.DangerousCreate(ref visitedTemp1D[0], span.Height, span.Width, 0);

        var (sgx, sgy) = (guardIdx % span.Width, guardIdx / span.Width);
        
        var checker = new P2CheckerInitial(span, spanT, visited, visitedTemp);

        Simulate(visited, ref checker, span, spanT, sgy, sgx, Direction.Up);

        return checker.Count; // 1767
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
}