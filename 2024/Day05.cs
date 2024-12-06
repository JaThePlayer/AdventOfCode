using System.Runtime.InteropServices;

namespace AoC._2024;

/*
 Initial
| Method | Mean      | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|------- |----------:|---------:|---------:|-------:|-------:|----------:|
| Part1  |  61.67 us | 0.200 us | 0.177 us | 2.6855 | 0.1221 |   22.8 KB |
| Part2  | 209.90 us | 0.808 us | 0.717 us | 2.6855 |      - |  23.91 KB |
 
 Part2: End sorting invalid values at the halfway point
| Method | Mean      | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|------- |----------:|---------:|---------:|-------:|-------:|----------:|
| Part1  |  61.51 us | 0.431 us | 0.403 us | 2.6855 | 0.1221 |   22.8 KB |
| Part2  | 162.54 us | 0.735 us | 0.688 us | 2.6855 |      - |  23.91 KB |

 Part2: Get rid of correctOrder list
| Method | Mean      | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|------- |----------:|---------:|---------:|-------:|-------:|----------:|
| Part1  |  62.08 us | 0.255 us | 0.226 us | 2.6855 | 0.1221 |   22.8 KB |
| Part2  | 166.88 us | 0.685 us | 0.607 us | 2.6855 |      - |  23.35 KB |

Part2: RemoveAt instead of Remove
| Method | Mean      | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|------- |----------:|---------:|---------:|-------:|-------:|----------:|
| Part1  |  63.23 us | 0.298 us | 0.264 us | 2.6855 | 0.1221 |   22.8 KB |
| Part2  | 157.49 us | 0.691 us | 0.612 us | 2.6855 |      - |  23.35 KB |

Part2: RemainingNumbers is now just a span, no removing from it
| Method | Mean      | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|------- |----------:|---------:|---------:|-------:|-------:|----------:|
| Part1  |  60.98 us | 0.294 us | 0.260 us | 2.7466 | 0.1221 |   22.8 KB |
| Part2  | 137.19 us | 0.478 us | 0.447 us | 2.6855 |      - |   22.8 KB |

Part2: Opt for i=0 in final loop
| Method | Mean      | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|------- |----------:|---------:|---------:|-------:|-------:|----------:|
| Part1  |  61.96 us | 0.475 us | 0.421 us | 2.6855 | 0.1221 |   22.8 KB |
| Part2  | 134.36 us | 0.653 us | 0.579 us | 2.6855 |      - |   22.8 KB |

Provide starting capacity for 'dependencies'
| Method | Mean      | Error    | StdDev   | Gen0   | Allocated |
|------- |----------:|---------:|---------:|-------:|----------:|
| Part1  |  64.10 us | 0.304 us | 0.270 us | 2.4414 |  20.02 KB |
| Part2  | 142.94 us | 0.889 us | 0.788 us | 2.4414 |  20.02 KB |

Use bytes instead of ints
| Method | Mean      | Error    | StdDev   | Gen0   | Allocated |
|------- |----------:|---------:|---------:|-------:|----------:|
| Part1  |  63.42 us | 0.568 us | 0.475 us | 1.3428 |  11.23 KB |
| Part2  | 141.33 us | 0.481 us | 0.450 us | 1.2207 |  11.23 KB |

Stackalloc the masks
| Method | Mean      | Error    | StdDev   | Gen0   | Allocated |
|------- |----------:|---------:|---------:|-------:|----------:|
| Part1  |  63.25 us | 0.484 us | 0.453 us | 1.3428 |   11.1 KB |
| Part2  | 141.52 us | 0.983 us | 0.919 us | 1.2207 |   11.1 KB |

Make the mask a constant 100 bytes.
| Method | Mean      | Error    | StdDev   | Gen0   | Allocated |
|------- |----------:|---------:|---------:|-------:|----------:|
| Part1  |  61.63 us | 1.193 us | 0.996 us | 1.3428 |   11.1 KB |
| Part2  | 137.74 us | 1.506 us | 1.176 us | 1.2207 |   11.1 KB |

Stackalloc the number list buffer
| Method | Mean      | Error    | StdDev   | Gen0   | Allocated |
|------- |----------:|---------:|---------:|-------:|----------:|
| Part1  |  60.15 us | 0.236 us | 0.221 us | 1.2817 |  10.95 KB |
| Part2  | 134.91 us | 0.502 us | 0.445 us | 1.2207 |  10.95 KB |

FastParse2DigitInt
| Method | Mean      | Error    | StdDev   | Gen0   | Allocated |
|------- |----------:|---------:|---------:|-------:|----------:|
| Part1  |  56.26 us | 0.164 us | 0.153 us | 1.2817 |  10.95 KB |
| Part2  | 133.84 us | 0.456 us | 0.405 us | 1.2207 |  10.95 KB |
 */

using TNum = byte;

public class Day05 : AdventBase
{
    public override int Year => 2024;
    public override int Day => 5;

    private static Dictionary<TNum, List<TNum>> ParseRules(ReadOnlySpan<char> input,
        out MemoryExtensions.SpanSplitEnumerator<char> remaining,
        out TNum maxSeenNumber)
    {
        Dictionary<TNum, List<TNum>> dependencies = new(49); // capacity should be 49, but that feels cheaty
        
        remaining = input.Split('\n').GetEnumerator();
        maxSeenNumber = 0;
        while (remaining.MoveNext())
        {
            var line = input[remaining.Current];
            if (line.IsWhiteSpace())
                break;
            var left = Util.FastParse2DigitInt<TNum>(line[..2]);
            var right = Util.FastParse2DigitInt<TNum>(line[3..]);
            maxSeenNumber = TNum.Max(maxSeenNumber, TNum.Max(left, right));

            ref var list = ref CollectionsMarshal.GetValueRefOrAddDefault(dependencies, right, out _);
            list ??= [];
            
            list.Add(left);
        }

        return dependencies;
    }

    private static bool CanAddNum(TNum num, Dictionary<TNum, List<TNum>> dependencies, Span<bool> invalidMask)
    {
        if (!dependencies.TryGetValue(num, out var deps))
            return true;
            
        foreach (var dep in deps)
        {
            if (dep < invalidMask.Length && invalidMask[dep])
                return false;
        }

        return true;
    }

    private static bool IsValid(Span<TNum> numbers, Dictionary<TNum, List<TNum>> dependencies, Span<bool> invalidMask, out int validNumbers)
    {
        validNumbers = 0;
        foreach (var num in numbers)
        {
            if (!CanAddNum(num, dependencies, invalidMask))
                return false;

            validNumbers++;
            invalidMask[num] = false;
        }

        return true;
    }
    
    protected override object Part1Impl()
    {
        var input = Input.Text.AsSpan();
        var dependencies = ParseRules(input, out var enumerable, out var maxSeenNumber);
        Span<TNum> buffer = stackalloc TNum[100];
        Span<bool> invalidMask = stackalloc bool[100]; // maxSeenNumber + 1
        var sum = 0;
        
        while (enumerable.MoveNext())
        {
            var line = input[enumerable.Current];
            var numbers = Util.FastParse2DigitIntList(line, buffer);

            invalidMask.Fill(false);
            foreach (var n in numbers)
            {
                if (n < invalidMask.Length)
                    invalidMask[n] = true;
            }

            if (IsValid(numbers, dependencies, invalidMask, out _))
                sum += numbers[numbers.Length / 2];
        }

        return sum; // 5087
    }

    protected override object Part2Impl()
    {
        var input = Input.Text.AsSpan();
        var dependencies = ParseRules(input, out var enumerable, out var maxSeenNumber);
        Span<TNum> buffer = stackalloc TNum[100];
        Span<bool> invalidMask = stackalloc bool[100]; // maxSeenNumber + 1
        var sum = 0;
        
        while (enumerable.MoveNext())
        {
            var line = input[enumerable.Current];
            var numbers = Util.FastParse2DigitIntList(line, buffer);
            
            invalidMask.Fill(false);
            foreach (var n in numbers)
            {
                if (n < invalidMask.Length)
                    invalidMask[n] = true;
            }
            
            if (IsValid(numbers, dependencies, invalidMask, out var correctOrderCount))
                continue;
            
            var remainingNumbers = numbers[correctOrderCount..];
            
            while (numbers.Length / 2 >= correctOrderCount)
            {
                for (var i = 0; i < remainingNumbers.Length; i++)
                {
                    var num = remainingNumbers[i];
                    
                    if (!invalidMask[num] || !CanAddNum(num, dependencies, invalidMask))
                        continue;

                    correctOrderCount++;
                    if (numbers.Length / 2 < correctOrderCount)
                    {
                        sum += num;
                        break;
                    }

                    invalidMask[num] = false;
                    if (i == 0)
                    {
                        remainingNumbers = remainingNumbers[1..];
                        i--;
                    }
                    break;
                }
            }
        }

        return sum; // 4971
    }
}