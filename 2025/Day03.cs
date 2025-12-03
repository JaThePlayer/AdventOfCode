using System.Numerics.Tensors;

namespace AoC._2025;

/*
Initial
| Method | Mean     | Error    | StdDev   | Allocated |
|------- |---------:|---------:|---------:|----------:|
| Part1  | 13.94 us | 0.193 us | 0.180 us |      24 B |
| Part2  | 34.06 us | 0.564 us | 0.500 us |      24 B |

P2 Stop searching after finding a 9. (regression in P1)
| Method | Mean     | Error    | StdDev   | Allocated |
|------- |---------:|---------:|---------:|----------:|
| Part1  | 14.39 us | 0.056 us | 0.049 us |      24 B |
| Part2  | 28.79 us | 0.492 us | 0.460 us |      24 B |

P1 - TensorPrimitives.Max to find 2nd digit.
| Method | Mean     | Error     | StdDev    | Allocated |
|------- |---------:|----------:|----------:|----------:|
| Part1  | 9.776 us | 0.0399 us | 0.0333 us |      24 B |

P1 - TensorPrimitives.IndexOfMax for 1st digit
| Method | Mean     | Error     | StdDev    | Allocated |
|------- |---------:|----------:|----------:|----------:|
| Part1  | 4.317 us | 0.0461 us | 0.0432 us |      24 B |

P2 - TensorPrimitives.IndexOfMax
| Method | Mean      | Error     | StdDev    | Allocated |
|------- |----------:|----------:|----------:|----------:|
| Part1  |  4.203 us | 0.0055 us | 0.0046 us |      24 B |
| Part2  | 22.416 us | 0.1922 us | 0.1797 us |      24 B |
 */
public class Day03 : AdventBase
{
    public override int Year => 2025;
    public override int Day => 3;
    
    protected override object Part1Impl()
    {
        var sum = 0;
        var input = Input.TextU8;
        foreach (var bankRange in input.Split((byte)'\n'))
        {
            var bank = input[bankRange];
            var tensDigitIdx = TensorPrimitives.IndexOfMax(bank[..^1]);
            var tensDigit = bank[tensDigitIdx];
            var bat2 = TensorPrimitives.Max(bank[(tensDigitIdx + 1)..]);

            var res = (tensDigit - '0') * 10 + (bat2 - '0');
            sum += res;
        }

        return sum; // 17196
    }

    protected override object Part2Impl()
    {
        ulong sum = 0;
        Span<byte> chars = stackalloc byte[12];
        var input = Input.TextU8;
        foreach (var bankRange in input.Split((byte)'\n'))
        {
            var bank = input[bankRange];
            var startIdx = 0;
            
            for (var charI = 0; charI < chars.Length; charI++)
            {
                startIdx += TensorPrimitives.IndexOfMax(bank[startIdx..^(chars.Length - charI - 1)]);
                chars[charI] = bank[startIdx];
                startIdx++;
            }

            sum += Util.FastParseInt<ulong>(chars);
        }

        return sum; // 171039099596062
    }
}