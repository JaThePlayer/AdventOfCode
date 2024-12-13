using CommunityToolkit.HighPerformance;

namespace AoC._2024;

/*
| Method | Mean     | Error   | StdDev  | Allocated |
|------- |---------:|--------:|--------:|----------:|
| Part1  | 550.7 us | 6.41 us | 5.99 us |      24 B |
| Part2  | 952.1 us | 2.93 us | 2.45 us |      24 B |

Part 2: Don't clear the directions map each iteration
| Method | Mean     | Error   | StdDev  | Allocated |
|------- |---------:|--------:|--------:|----------:|
| Part1  | 556.7 us | 4.79 us | 4.00 us |      24 B |
| Part2  | 849.0 us | 8.87 us | 8.30 us |      24 B |

Stupid generics
| Method | Mean     | Error   | StdDev  | Allocated |
|------- |---------:|--------:|--------:|----------:|
| Part1  | 257.9 us | 2.65 us | 2.35 us |      24 B |
| Part2  | 540.2 us | 3.39 us | 3.01 us |      24 B |

Microopts
| Method | Mean     | Error   | StdDev  | Allocated |
|------- |---------:|--------:|--------:|----------:|
| Part1  | 230.3 us | 1.19 us | 0.99 us |      24 B |
| Part2  | 495.4 us | 1.12 us | 0.99 us |      24 B |
 */

public class Day12 : AdventBase
{
    public override int Year => 2024;
    public override int Day => 12;

    private (int, int) HandleRegion<TDirs>(ReadOnlySpan2D<char> map, Span2D<bool> visited, char tile, int sx, int sy)
        where TDirs : IDirPicker
    {
        visited[sy, sx] = true;

        var area = 1;
        var perimeter = 0;

        if (TDirs.Left)
            Visit<ExceptRightPicker>(map, visited, sx - 1, sy);
        if (TDirs.Right)
            Visit<ExceptLeftPicker>(map, visited, sx + 1, sy);
        if (TDirs.Down)
            Visit<ExceptUpPicker>(map, visited, sx, sy + 1);
        if (TDirs.Up)
            Visit<ExceptDownPicker>(map, visited, sx, sy - 1);
        
        return (area, perimeter);

        void Visit<TDirs>(ReadOnlySpan2D<char> map, Span2D<bool> visited, int x, int y)
            where TDirs : IDirPicker
        {
            unchecked
            {
                if ((!TDirs.Left || !TDirs.Right) && (uint)x >= map.Width - 1)
                {
                    perimeter++;
                    return;
                }
                
                if ((!TDirs.Down || !TDirs.Up) && (uint)y >= map.Height)
                {
                    perimeter++;
                    return;
                }
            }
            
            if (map.DangerousGetReferenceAt(y, x) == tile)
            {
                if (visited.DangerousGetReferenceAt(y, x))
                    return;
                var (iarea, iper) = HandleRegion<TDirs>(map, visited, tile, x, y);
                area += iarea;
                perimeter += iper;
            }
            else
            {
                perimeter++;
            }
        }
    }
    
    protected override object Part1Impl()
    {
        ulong sum = 0;
        var input = Input.Text.AsSpan();
        var lineWidth = input.IndexOf('\n') + 1;
        
        var span = ReadOnlySpan2D<char>.DangerousCreate(input[0], input.Length / lineWidth + 1, lineWidth, 0);
        Span<bool> visited1D = stackalloc bool[span.Height * span.Width];
        var visited = Span2D<bool>.DangerousCreate(ref visited1D[0], span.Height, span.Width, 0);

        for (int y = 0; y < span.Height; y++)
        {
            for (int x = 0; x < span.Width - 1; x++)
            {
                if (visited[y, x])
                    continue;
                var (area, perimeter) = HandleRegion<AllDirPicker>(span, visited, span[y, x], x, y);
                sum += (ulong)area * (ulong)perimeter;
            }
        }

        return sum; // 1461806
    }

    private (int, int) HandleRegionPart2<TDirs>(ReadOnlySpan2D<char> map, Span2D<bool> visited, Span2D<Direction> fences, char tile, int sx, int sy)
        where TDirs : IDirPicker
    {
        visited.DangerousGetReferenceAt(sy, sx) = true;

        var area = 1;
        var sides = 0;

        if (TDirs.Left)
            Visit<ExceptRightPicker>(map, visited, fences, sx - 1, sy, Direction.Left);
        if (TDirs.Right)
            Visit<ExceptLeftPicker>(map, visited, fences, sx + 1, sy, Direction.Right);
        if (TDirs.Down)
            Visit<ExceptUpPicker>(map, visited, fences, sx, sy + 1, Direction.Down);
        if (TDirs.Up)
            Visit<ExceptDownPicker>(map, visited, fences, sx, sy - 1, Direction.Up);
        
        return (area, sides);

        void Visit<TDirs>(ReadOnlySpan2D<char> map, Span2D<bool> visited, Span2D<Direction> fences, int x, int y, Direction dir)
            where TDirs : IDirPicker
        {
            unchecked
            {
                if ((!TDirs.Left || !TDirs.Right) && (uint)x >= map.Width - 1)
                    goto foundBorder;
                if ((!TDirs.Down || !TDirs.Up) && (uint)y >= map.Height)
                    goto foundBorder;
            }
            
            if (map.DangerousGetReferenceAt(y, x) == tile)
            {
                if (visited.DangerousGetReferenceAt(y, x))
                    return;
                var (iarea, iper) = HandleRegionPart2<TDirs>(map, visited, fences, tile, x, y);
                area += iarea;
                sides += iper;
                return;
            }
            
            foundBorder:
            ref var fence = ref fences.DangerousGetReferenceAt(sy, sx);
            if ((fence & dir) == dir)
                return;
            
            sides++;
            fence |= dir;
            
            unchecked
            {
                switch (dir)
                {
                    case Direction.Right or Direction.Left:
                        var ny = sy + 1;
                        if ((uint)x >= map.Width - 1)
                        {
                            // we're checking OOB, so we can definitely extend the fence
                            while ((uint)ny < map.Height && map.DangerousGetReferenceAt(ny, sx) == tile)
                            {
                                fences.DangerousGetReferenceAt(ny, sx) |= dir;
                                ny++;
                            }
                            ny = sy - 1;
                            while (ny >= 0 && map.DangerousGetReferenceAt(ny, sx) == tile)
                            {
                                fences.DangerousGetReferenceAt(ny, sx) |= dir;
                                ny--;
                            }

                            break;
                        }
                        
                        while ((uint)ny < map.Height && map.DangerousGetReferenceAt(ny, sx) == tile && map.DangerousGetReferenceAt(ny, x) != tile)
                        {
                            fences.DangerousGetReferenceAt(ny, sx) |= dir;
                            ny++;
                        }
                        ny = sy - 1;
                        while (ny >= 0 && map.DangerousGetReferenceAt(ny, sx) == tile && map.DangerousGetReferenceAt(ny, x) != tile)
                        {
                            fences.DangerousGetReferenceAt(ny, sx) |= dir;
                            ny--;
                        }
                        break;
                    default:
                        var nx = sx + 1;
                        var selfRow = map.GetRowSpan(sy);
                        var fenceRow = fences.GetRowSpan(sy);
                        if ((uint)y >= map.Height)
                        {
                            while ((uint)nx < selfRow.Length && selfRow.DangerousGetReferenceAt(nx) == tile)
                            {
                                fenceRow.DangerousGetReferenceAt(nx) |= dir;
                                nx++;
                            }
                            nx = sx - 1;
                            while (nx >= 0 && selfRow.DangerousGetReferenceAt(nx) == tile)
                            {
                                fenceRow.DangerousGetReferenceAt(nx) |= dir;
                                nx--;
                            }
                            break;
                        }
                        
                        var checkedRow = map.GetRowSpan(y);
                        while ((uint)nx < checkedRow.Length && selfRow.DangerousGetReferenceAt(nx) == tile && checkedRow.DangerousGetReferenceAt(nx) != tile)
                        {
                            fenceRow.DangerousGetReferenceAt(nx) |= dir;
                            nx++;
                        }
                        nx = sx - 1;
                        while (nx >= 0 && selfRow.DangerousGetReferenceAt(nx) == tile && checkedRow.DangerousGetReferenceAt(nx) != tile)
                        {
                            fenceRow.DangerousGetReferenceAt(nx) |= dir;
                            nx--;
                        }
                        break;
                }
            }
        }
    }
    
    protected override object Part2Impl()
    {
        ulong sum = 0;
        var input = Input.Text.AsSpan();
        var lineWidth = input.IndexOf('\n') + 1;
        
        var span = ReadOnlySpan2D<char>.DangerousCreate(input[0], input.Length / lineWidth + 1, lineWidth, 0);
        Span<bool> visited1D = stackalloc bool[span.Height * span.Width];
        var visited = Span2D<bool>.DangerousCreate(ref visited1D[0], span.Height, span.Width, 0);
        Span<Direction> fences1D = stackalloc Direction[span.Height * span.Width];
        var fences = Span2D<Direction>.DangerousCreate(ref fences1D[0], span.Height, span.Width, 0);

        for (var y = 0; y < span.Height; y++)
        {
            for (var x = 0; x < span.Width - 1; x++)
            {
                if (visited[y, x])
                    continue;
                
                var (area, sides) = HandleRegionPart2<AllDirPicker>(span, visited, fences, span[y, x], x, y);
                sum += (ulong)area * (ulong)sides;
            }
        }

        return sum; // 887932
    }
}