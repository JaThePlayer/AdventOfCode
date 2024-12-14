using CommunityToolkit.HighPerformance;

namespace AoC._2024;

/*
Initial
| Method | Mean          | Error      | StdDev     | Allocated |
|------- |--------------:|-----------:|-----------:|----------:|
| Part1  |      17.44 us |   0.231 us |   0.205 us |      24 B |
| Part2  | 236,119.68 us | 614.963 us | 513.522 us |      61 B |
 */
public class Day14 : AdventBase
{
    const int width = 101;
    const int height = 103;
    
    public override int Year => 2024;
    public override int Day => 14;
    
    protected override object Part1Impl()
    {
        var input = Input.TextU8;

        var q1 = Simulate(input, 100, out var q2, out var q3, out var q4, default);

        return q1 * q2 * q3 * q4; // 216772608
    }

    private static int Simulate(ReadOnlySpan<byte> input, int steps, out int q2, out int q3, out int q4, Span2D<byte> outputRobotCount)
    {
        var q1 = 0;
        q2 = 0;
        q3 = 0;
        q4 = 0;
        
        foreach (var lineRange in input.Split((byte)'\n'))
        {
            var line = input[lineRange];

            line.SplitTwo((byte)' ', out var posSpan, out var velSpan);
            posSpan = posSpan.Slice(2);
            velSpan = velSpan.Slice(2);
            posSpan.SplitTwo((byte)',', out var xs, out var ys);
            velSpan.SplitTwo((byte)',', out var vxs, out var vys);
            var x = int.Parse(xs);
            var y = int.Parse(ys);
            var vx = int.Parse(vxs);
            var vy = int.Parse(vys);
            
            x += steps * vx;
            y += steps * vy;
            
            x %= width;
            y %= height;
            
            if (y < 0)
                y = height + y;
            if (x < 0)
                x = width + x;

            if (outputRobotCount != default)
                outputRobotCount[y, x]++;

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
        
        return q1;
    }

    protected override object Part2Impl()
    {
        Span<byte> robotCount1d = stackalloc byte[width * height];
        Span2D<byte> robotCount = Span2D<byte>.DangerousCreate(ref robotCount1d[0], height, width, 0);
        
        for (int i = 0; i < 100000000; i++)
        {
            robotCount.Fill(0);
            Simulate(Input.TextU8, i, out var q2, out var q3, out var q4, robotCount);

            for (int y = 0; y < height; y++)
            {
                var row = robotCount.GetRowSpan(y);
                var hitsInRow = 0;
                for (int x = 0; x < width; x++)
                {
                    var c = row.DangerousGetReferenceAt(x);
                    if (c > 0)
                    {
                        hitsInRow++;
                        if (hitsInRow >= "################".Length)
                            return i; // 6888
                    }
                    else
                    {
                        hitsInRow = 0;
                    }
                }
            }
        }

        return -1;
    }
}