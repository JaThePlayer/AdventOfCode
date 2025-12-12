using System.Numerics.Tensors;

namespace AoC._2025;

/*
| Method | Mean           | Error       | StdDev      | Median         | Allocated |
|------- |---------------:|------------:|------------:|---------------:|----------:|
| Part1  | 31,085.4818 ns | 135.1464 ns | 112.8534 ns | 31,053.0548 ns |      24 B |
| Part2  |      0.0368 ns |   0.0279 ns |   0.0248 ns |      0.0220 ns |         - |
 */
public class Day12 : AdventBase
{
    public override int Year => 2025;
    public override int Day => 12;
    
    protected override object Part1Impl()
    {
        var input = Input.TextU8;

        var p = new SpanParserU8(input);
        var res = 0;
        Span<int> buffer = stackalloc int[6];
        while (true)
        {
            if (p.IsEmpty)
                break;
            var header = p.ReadStrUntil(':');
            if (!header.Contains((byte)'x'))
            {
                p.ReadStrUntil("\n\n"u8);
                continue;
            }
            
            // 40x48: 34 38 38 31 28 38
            var (w, h) = Util.FastParseIntPair<int, byte>(header, (byte)'x');
            var remaining = p.ReadStrUntil('\n').Trim((byte)' ');
            var counts = Util.FastParseIntList(remaining, (byte)' ', buffer);

            if (TensorPrimitives.Sum(counts) * 9 <= w * h)
                res++;
        }
        
        return res; // 538
    }

    protected override object Part2Impl()
    {
        return "";
    }
}