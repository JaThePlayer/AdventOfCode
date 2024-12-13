using System.Numerics.Tensors;
using System.Runtime.CompilerServices;

namespace AoC._2024;

/*
| Method | Mean     | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|------- |---------:|---------:|---------:|-------:|-------:|----------:|
| Part1  | 31.15 us | 0.293 us | 0.260 us | 0.9155 |      - |   7.88 KB |
| Part2  | 27.22 us | 0.085 us | 0.066 us | 3.6011 | 0.0916 |  29.55 KB |

manual simd
| Method | Mean     | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|------- |---------:|---------:|---------:|-------:|-------:|----------:|
| Part1  | 30.92 us | 0.255 us | 0.226 us | 0.9155 |      - |   7.88 KB |
| Part2  | 27.31 us | 0.156 us | 0.138 us | 3.6011 | 0.0916 |  29.55 KB |

Unsafe.Add and refs in ParseInput
| Method | Mean     | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|------- |---------:|---------:|---------:|-------:|-------:|----------:|
| Part1  | 30.62 us | 0.297 us | 0.278 us | 0.9155 |      - |   7.88 KB |
| Part2  | 27.44 us | 0.123 us | 0.115 us | 3.6011 | 0.0916 |  29.55 KB |

Part2 rework Use sorting:
| Method | Mean     | Error    | StdDev   | Gen0   | Allocated |
|------- |---------:|---------:|---------:|-------:|----------:|
| Part1  | 31.95 us | 0.231 us | 0.216 us | 0.9155 |   7.88 KB |
| Part2  | 32.15 us | 0.176 us | 0.147 us | 0.9155 |   7.88 KB |

FastIntParse
| Method | Mean     | Error    | StdDev   | Gen0   | Allocated |
|------- |---------:|---------:|---------:|-------:|----------:|
| Part1  | 21.10 us | 0.091 us | 0.076 us | 0.9460 |   7.88 KB |
| Part2  | 22.59 us | 0.151 us | 0.141 us | 0.9460 |   7.88 KB |
 */
public class Day01 : AdventBase
{
    public override int Year => 2024;
    public override int Day => 1;

    protected (int[] leftNums, int[] rightNums) ParseInput()
    {
        ReadOnlySpan<byte> input = Input.TextU8;
        
        var lineCount = input.Count((byte)'\n') + 1;
        var leftNums = new int[lineCount];
        var rightNums = new int[lineCount];
        
        ref var left = ref leftNums[0];
        ref var right = ref rightNums[0];
        
        foreach (var lineRange in input.Split((byte)'\n'))
        {
            var line = input[lineRange];
            left = Util.FastParseInt<int>(line[..5]);
            right = Util.FastParseInt<int>(line[8..]);
            
            left = ref Unsafe.Add(ref left, 1);
            right = ref Unsafe.Add(ref right, 1);
        }
        
        return (leftNums, rightNums);
    }
    
    protected override object Part1Impl()
    {
        var (leftNums, rightNums) = ParseInput();
        var leftSpan = leftNums.AsSpan();
        var rightSpan = rightNums.AsSpan();
        
        leftSpan.Sort();
        rightSpan.Sort();
        
        
        TensorPrimitives.Subtract(leftSpan, rightSpan, leftSpan);
        return TensorPrimitives.SumOfMagnitudes<int>(leftSpan); // 936063
    }

    protected override object Part2Impl()
    {
        var (leftNums, rightNums) = ParseInput();
        var leftSpan = leftNums.AsSpan();
        var rightSpan = rightNums.AsSpan();
        
        leftSpan.Sort();
        rightSpan.Sort();

        if (leftNums.Length != rightNums.Length)
            return -1;

        var sum = 0;

        var i = 0;
        var j = 0;
        var right = rightSpan[j];

        while ((uint)i < leftSpan.Length)
        {
            var left = leftSpan[i];
            var mult = 1;
            while ((uint)(i + 1) < leftSpan.Length && leftSpan[i+1] == left)
            {
                mult++;
                i++;
            }
            
            while (left > right && (uint)(j + 1) < rightSpan.Length)
            {
                j++;
                right = rightSpan[j];
            }

            if (left == right)
            {
                while ((uint)(j + 1) < rightSpan.Length && rightSpan[j+1] == left)
                {
                    mult++;
                    j++;
                }
                sum += left * mult;
            }

            i++;
        }

        return sum; // 23150395
    }
}