using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

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
    static bool ParsePrizeLine(ref ReadOnlySpan<byte> input, out long px, out long py)
    {
        var splitIdx = input.IndexOf((byte)',');
        px = Util.FastParseInt<long>(input[..splitIdx]);
        input = input.Slice(splitIdx + 4);
        splitIdx = input.IndexOf((byte)'\n');
        if (splitIdx == -1)
        {
            py = Util.FastParseInt<long>(input);
            return true;
        }

        py = Util.FastParseInt<long>(input[..splitIdx]);
        input = input.Slice(splitIdx + 2);
        return false;
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
            var ax = Util.FastParse2DigitInt<int>(input, "Button A: X+".Length);
            var ay = Util.FastParse2DigitInt<int>(input, "Button A: X+26, Y+".Length);
            var bx = Util.FastParse2DigitInt<int>(input, "Button A: X+26, Y+57\nButton A: X+".Length);
            var by = Util.FastParse2DigitInt<int>(input, "Button A: X+26, Y+57\nButton A: X+26, Y+".Length);
            input = input.Slice("Button A: X+26, Y+57\nButton A: X+26, Y+57\nPrize: X=".Length);
            var shouldExit = ParsePrizeLine(ref input, out var px, out var py);
            px += TOffset.Value;
            py += TOffset.Value;
            sum += FindSolution(ax, ay, bx, by, px, py);

            if (shouldExit)
                return sum;
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