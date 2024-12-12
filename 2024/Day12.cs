using CommunityToolkit.HighPerformance;

namespace AoC._2024;

/*
| Method | Mean     | Error   | StdDev  | Allocated |
|------- |---------:|--------:|--------:|----------:|
| Part1  | 550.7 us | 6.41 us | 5.99 us |      24 B |
| Part2  | 952.1 us | 2.93 us | 2.45 us |      24 B |
 */

public class Day12 : AdventBase
{
    public override int Year => 2024;
    public override int Day => 12;

    private (int, int) HandleRegion(ReadOnlySpan2D<char> map, Span2D<bool> visited, char tile, int sx, int sy)
    {
        visited[sy, sx] = true;

        var area = 1;
        var perimeter = 0;

        Visit(map, visited, sx - 1, sy);
        Visit(map, visited, sx + 1, sy);
        Visit(map, visited, sx, sy + 1);
        Visit(map, visited, sx, sy - 1);
        
        return (area, perimeter);

        void Visit(ReadOnlySpan2D<char> map, Span2D<bool> visited, int x, int y)
        {
            unchecked
            {
                if ((uint)x >= map.Width-1 || (uint)y >= map.Height)
                {
                    perimeter++;
                    return;
                }
            }
            
            if (map[y, x] == tile)
            {
                if (visited[y, x])
                    return;
                var (iarea, iper) = HandleRegion(map, visited, tile, x, y);
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
                var (area, perimeter) = HandleRegion(span, visited, span[y, x], x, y);
                sum += (ulong)area * (ulong)perimeter;
            }
        }

        return sum;
    }

    [Flags]
    enum Direction : byte
    {
        None = 0,
        Up = 1, 
        Down = 2, 
        Left = 4, 
        Right = 8,
    }

    private (int, int) HandleRegionPart2(ReadOnlySpan2D<char> map, Span2D<bool> visited, Span2D<Direction> fences, char tile, int sx, int sy)
    {
        visited[sy, sx] = true;

        var area = 1;
        var sides = 0;

        Visit(map, visited, fences, sx - 1, sy, Direction.Left);
        Visit(map, visited, fences, sx + 1, sy, Direction.Right);
        Visit(map, visited, fences, sx, sy + 1, Direction.Down);
        Visit(map, visited, fences, sx, sy - 1, Direction.Up);
        
        return (area, sides);

        void Visit(ReadOnlySpan2D<char> map, Span2D<bool> visited, Span2D<Direction> fences, int x, int y, Direction dir)
        {
            unchecked
            {
                if ((uint)x >= map.Width-1 || (uint)y >= map.Height)
                {
                    goto foundBorder;
                }
            }
            
            if (map[y, x] == tile)
            {
                if (visited[y, x])
                    return;
                var (iarea, iper) = HandleRegionPart2(map, visited, fences, tile, x, y);
                area += iarea;
                sides += iper;
                return;
            }
            
            foundBorder:
            if ((fences[sy, sx] & dir) == dir)
                return;
            
            sides++;
            fences[sy, sx] |= dir;
            unchecked
            {
                switch (dir)
                {
                    case Direction.Right or Direction.Left:
                        var ny = sy + 1;
                        while ((uint)ny < map.Height && map[ny, sx] == tile 
                                                     && ((uint)x >= map.Width-1 || map[ny, x] != tile))
                        {
                            fences[ny, sx] |= dir;
                            ny++;
                        }
                        ny = sy - 1;
                        while (ny >= 0 && map[ny, sx] == tile 
                                      && ((uint)x >= map.Width-1 || map[ny, x] != tile))
                        {
                            fences[ny, sx] |= dir;
                            ny--;
                        }
                        break;
                    case Direction.Up or Direction.Down:
                        var nx = sx + 1;
                        while ((uint)nx < map.Width && map[sy, nx] == tile 
                           && ((uint)y >= map.Height || map[y, nx] != tile))
                        {
                            fences[sy, nx] |= dir;
                            nx++;
                        }
                        nx = sx - 1;
                        while (nx >= 0 && map[sy, nx] == tile 
                                      && ((uint)y >= map.Height || map[y, nx] != tile))
                        {
                            fences[sy, nx] |= dir;
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
                fences.Fill(Direction.None);
                
                var (area, sides) = HandleRegionPart2(span, visited, fences, span[y, x], x, y);
                sum += (ulong)area * (ulong)sides;
            }
        }

        return sum;
    }
}