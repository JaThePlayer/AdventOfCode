using CommunityToolkit.HighPerformance;
using MemoryExtensions = System.MemoryExtensions;

namespace AoC._2024;

/*
Initial
| Method | Mean          | Error      | StdDev     | Allocated |
|------- |--------------:|-----------:|-----------:|----------:|
| Part1  |      17.44 us |   0.231 us |   0.205 us |      24 B |
| Part2  | 236,119.68 us | 614.963 us | 513.522 us |      61 B |

Part 2 rework
| Method | Mean      | Error    | StdDev   | Gen0   | Allocated |
|------- |----------:|---------:|---------:|-------:|----------:|
| Part1  |  17.91 us | 0.244 us | 0.216 us |      - |      24 B |
| Part2  | 549.04 us | 1.473 us | 1.378 us | 1.9531 |   16568 B |
 */
public class Day14 : AdventBase
{
    const int width = 101;
    const int height = 103;
    
    public override int Year => 2024;
    public override int Day => 14;
    
    private static void ParseRobot(ReadOnlySpan<byte> line, out int x, out int y, out int vx, out int vy)
    {
        line.SplitTwo((byte)' ', out var posSpan, out var velSpan);
        posSpan = posSpan.Slice(2);
        velSpan = velSpan.Slice(2);
        posSpan.SplitTwo((byte)',', out var xs, out var ys);
        velSpan.SplitTwo((byte)',', out var vxs, out var vys);
        x = int.Parse(xs);
        y = int.Parse(ys);
        vx = int.Parse(vxs);
        vy = int.Parse(vys);
    }
    
    protected override object Part1Impl()
    {
        var input = Input.TextU8;
        const int steps = 100;

        var q1 = 0;
        var q2 = 0;
        var q3 = 0;
        var q4 = 0;
        
        foreach (var lineRange in input.Split((byte)'\n'))
        {
            var line = input[lineRange];

            ParseRobot(line, out var x, out var y, out var vx, out var vy);
            
            x += steps * vx;
            y += steps * vy;
            
            x %= width;
            y %= height;
            
            if (y < 0)
                y = height + y;
            if (x < 0)
                x = width + x;

            if (x > width / 2)
            {
                if (y > height / 2)
                    q4++;
                else if (y != height / 2)
                    q1++;
            }
            else if (x != width / 2)
            {
                if (y > height / 2)
                    q3++;
                else if (y != height / 2)
                    q2++;
            } 
        }

        return q1 * q2 * q3 * q4; // 216772608
    }
    
    private static void SimulatePart2(List<(int x, int y, int vx, int vy)> input, int steps, Span2D<byte> outputRobotCount)
    {
        foreach (var p in input)
        {
            var x = p.x;
            var y = p.y;
            x += steps * p.vx;
            y += steps * p.vy;
            
            x %= width;
            y %= height;
            
            if (y < 0)
                y = height + y;
            if (x < 0)
                x = width + x;

            outputRobotCount[y, x]++;
        }
    }

    protected override object Part2Impl()
    {
        Span<byte> robotCount1d = stackalloc byte[width * height];
        Span2D<byte> robotCount = Span2D<byte>.DangerousCreate(ref robotCount1d[0], height, width, 0);

        List<(int x, int y, int vx, int vy)> robots = new();
        var input = Input.TextU8;
        foreach (var lineRange in input.Split((byte)'\n'))
        {
            var line = input[lineRange];
            
            ParseRobot(line, out var x, out var y, out var vx, out var vy);
            robots.Add((x, y, vx, vy));
        }

        var rowsRepeat = -1;
        var colsRepeat = -1;
        
        // Because all robots repeat their x/y position every width/height iterations,
        // we can find a number which has the same modulo for our target cols and rows, and it will probably be a tree
        // We can assume that if there's a high robot concentration on a given row/column,
        // its probably going to form a tree in n iterations, once they synchronize with the robots in the other direction.
        for (int i = 0; i < height; i++)
        {
            robotCount.Fill(0);
            SimulatePart2(robots, i, robotCount);

            if (rowsRepeat == -1)
                for (int y = 0; y < height; y++)
                {
                    ReadOnlySpan<byte> row = robotCount.GetRowSpan(y);
                    var c = row.Length - MemoryExtensions.Count(row, (byte)0);
                    if (c >= 20)
                    {
                        rowsRepeat = i;
                        if (colsRepeat != -1)
                            i = int.MaxValue - 1;
                        break;
                    }
                }
            if (colsRepeat == -1)
                for (int x = 0; x < width; x++)
                {
                    var c = 0;
                    for (int y = 0; y < height; y++)
                    {
                        if (robotCount[y, x] != 0)
                            c++;
                    }
                    if (c >= 20)
                    {
                        colsRepeat = i;
                        if (rowsRepeat != -1)
                            i = int.MaxValue - 1;
                        break;
                    }
                }
        }

        // Chineese remainder theorem: bad version
        var k = rowsRepeat;
        while (true)
        {
            k += height;
            if (k % width == colsRepeat)
                break;
        }

        return k; // 6888
    }
}