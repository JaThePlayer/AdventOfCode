using System.Numerics;
using System.Runtime.CompilerServices;

namespace AoC._2024;

/*
Initial
| Method | Mean     | Error    | StdDev   | Allocated |
|------- |---------:|---------:|---------:|----------:|
| Part1  | 11.12 us | 0.063 us | 0.053 us |      24 B |
| Part2  | 12.12 us | 0.108 us | 0.101 us |      24 B |

Utf8
| Method | Mean     | Error    | StdDev   | Allocated |
|------- |---------:|---------:|---------:|----------:|
| Part1  | 10.29 us | 0.163 us | 0.152 us |      24 B |
| Part2  | 10.31 us | 0.108 us | 0.096 us |      24 B |

Input parsing rewrite
| Method | Mean     | Error     | StdDev    | Allocated |
|------- |---------:|----------:|----------:|----------:|
| Part1  | 4.067 us | 0.0616 us | 0.0546 us |      24 B |
| Part2  | 4.160 us | 0.0236 us | 0.0210 us |      24 B |
 */
public class Day13 : AdventBase
{
    public override int Year => 2024;
    public override int Day => 13;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void ParseButtonLine(ReadOnlySpan<byte> line, out int x, out int y) {
        (x, y) = (
            Util.FastParse2DigitInt<int>(line.Slice("Button A: X+".Length)),
            Util.FastParse2DigitInt<int>(line.Slice("Button A: X+26, Y+".Length))
        );
    }

    static long FindSolution(int ax, int ay, int bx, int by, long px, long py)
    {
        // Cramer's rule
        var detA = ax * by - bx * ay;
        var detAa = px * by - bx * py;
        if (detAa % detA != 0)
            return 0;
        
        var detAb = ax * py - px * ay;
        if (detAb % detA != 0)
            return 0;

        var a = detAa / detA;
        var b = detAb / detA;

        return 3*a + b;
    }

    struct ConstZero : IConst<long>
    {
        public static long Value => 0;
    }
    
    struct Part2Offset : IConst<long>
    {
        public static long Value => 10000000000000;
    }
    
    private long Impl<TOffset>()
        where TOffset : struct, IConst<long>
    {
        var input = Input.TextU8;
        long sum = 0;
        
        while (true)
        {
            ParseButtonLine(input, out var ax, out var ay);
            input = input.Slice("Button A: X+26, Y+57\n".Length);
            ParseButtonLine(input, out var bx, out var by);
            input = input.Slice("Button A: X+26, Y+57\nPrize: X=".Length);

            var splitIdx = input.IndexOf((byte)',');
            var px = Util.FastParseInt<long>(input[..splitIdx]) + TOffset.Value;
            input = input.Slice(splitIdx + 4);
            splitIdx = input.IndexOf((byte)'\n');
            long py;
            if (splitIdx == -1)
            {
                py = Util.FastParseInt<long>(input) + TOffset.Value;
            
                sum += FindSolution(ax, ay, bx, by, px, py);
                return sum;
            }
            
            py = Util.FastParseInt<long>(input[..splitIdx]) + TOffset.Value;
            sum += FindSolution(ax, ay, bx, by, px, py);
            
            input = input.Slice(splitIdx + 2);
        }
    }
    
    protected override object Part1Impl()
    {
        return Impl<ConstZero>(); // 32026
    }

    protected override object Part2Impl()
    {
        return Impl<Part2Offset>(); // 89013607072065
    }
}