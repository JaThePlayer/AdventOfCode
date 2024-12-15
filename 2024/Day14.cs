using System.Runtime.Intrinsics;
using CommunityToolkit.HighPerformance;
using MemoryExtensions = System.MemoryExtensions;

namespace AoC._2024;

using Vec = Vector256;
using VecInt = Vector256<int>;

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

Part 2 rework 2
| Part2  | 231.5 us | 0.79 us | 0.74 us |      24 B |

Part 2 SIMD
| Method | Mean     | Error    | StdDev   | Allocated |
|------- |---------:|---------:|---------:|----------:|
| Part1  | 17.92 us | 0.345 us | 0.355 us |      24 B |
| Part2  | 95.31 us | 0.636 us | 0.595 us |      24 B |
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
    
    private static void SimulatePart2(Span<int> xs, Span<int> ys, Span<int> vxs, Span<int> vys, Span<byte> rowCounts, Span<byte> colCounts)
    {
        var i = 0;

        if (ys.Length != xs.Length)
            return;
        
        while (i + VecInt.Count < xs.Length)
        {
            var x = Vec.LoadUnsafe(ref xs.DangerousGetReferenceAt(i));
            var y = Vec.LoadUnsafe(ref ys.DangerousGetReferenceAt(i));
            
            x += Vec.LoadUnsafe(ref vxs.DangerousGetReferenceAt(i));
            y += Vec.LoadUnsafe(ref vys.DangerousGetReferenceAt(i));
            x += Vec.GreaterThanOrEqual(x, Vec.Create(width)) * width;
            y += Vec.GreaterThanOrEqual(y, Vec.Create(height)) * height;
            x += Vec.LessThan(x, Vec.Create(0)) * -width;
            y += Vec.LessThan(y, Vec.Create(0)) * -height;
            
            x.StoreUnsafe(ref xs.DangerousGetReferenceAt(i));
            y.StoreUnsafe(ref ys.DangerousGetReferenceAt(i));

            for (var j = i + VecInt.Count - 1; j >= i; j--)
            {
                colCounts.DangerousGetReferenceAt(xs.DangerousGetReferenceAt(j))++;
                rowCounts.DangerousGetReferenceAt(ys.DangerousGetReferenceAt(j))++;
            }
            
            i += VecInt.Count;
        }
        
        while (i <  xs.Length)
        {
            ref var x = ref xs.DangerousGetReferenceAt(i);
            ref var y = ref ys.DangerousGetReferenceAt(i);
            x += vxs.DangerousGetReferenceAt(i);
            y += vys.DangerousGetReferenceAt(i);
            
            x %= width;
            y %= height;
            if (y < 0)
                y = height + y;
            if (x < 0)
                x = width + x;

            rowCounts[y]++;
            colCounts[x]++;
            i++;
        }
    }

    protected override object Part2Impl()
    {
        var input = Input.TextU8;
        
        var robotCount = MemoryExtensions.Count(input, (byte)'\n') + 1;
        
        Span<int> xs = stackalloc int[robotCount];
        Span<int> ys = stackalloc int[robotCount];
        Span<int> vxs = stackalloc int[robotCount];
        Span<int> vys = stackalloc int[robotCount];
        var ri = 0;
        
        foreach (var lineRange in input.Split((byte)'\n'))
        {
            ParseRobot(input[lineRange], out xs[ri], out ys[ri], out vxs[ri], out vys[ri]);
            ri++;
        }

        var rowsRepeat = -1;
        var colsRepeat = -1;
        
        Span<byte> rowCounts = stackalloc byte[height];
        Span<byte> colCounts = stackalloc byte[width];
        
        // Because all robots repeat their x/y position every width/height iterations,
        // we can find a number which has the same modulo for our target cols and rows, and it will probably be a tree
        // We can assume that if there's a high robot concentration on a given row/column,
        // its probably going to form a tree in n iterations, once they synchronize with the robots in the other direction.
        for (int i = 1; i < height; i++)
        {
            rowCounts.Fill(0);
            colCounts.Fill(0);
            SimulatePart2(xs, ys, vxs, vys, rowCounts, colCounts);
            if (rowsRepeat == -1)
                for (int y = 0; y < height; y++)
                {
                    var c = rowCounts[y];
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
                    var c = colCounts[x];
                    if (c >= 20)
                    {
                        colsRepeat = i;
                        if (rowsRepeat != -1)
                            i = int.MaxValue - 1;
                        break;
                    }
                }
        }

        // Chinese remainder theorem: bad version
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