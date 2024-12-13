using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AoC._2024;

/*
Thanks to Simon on Celestecord for the idea.
 
 Initial - clean pattern matching
| Method | Mean      | Error    | StdDev   | Allocated |
|------- |----------:|---------:|---------:|----------:|
| Part1  |  83.94 us | 1.595 us | 1.706 us |     144 B |
| Part2  | 115.19 us | 2.229 us | 2.085 us |     144 B |

 BS
| Method | Mean     | Error    | StdDev   | Allocated |
|------- |---------:|---------:|---------:|----------:|
| Part1  | 60.54 us | 1.199 us | 1.601 us |     144 B |
| Part2  | 92.70 us | 1.711 us | 1.601 us |     144 B |

Utf8
| Method | Mean     | Error    | StdDev   | Allocated |
|------- |---------:|---------:|---------:|----------:|
| Part1  | 58.47 us | 1.166 us | 1.388 us |     144 B |
| Part2  | 86.40 us | 1.723 us | 3.746 us |     144 B |
 */
public class Day07Backwards : Day07
{
    private static bool TryPart1(ulong target, Span<ulong> numbers, int i)
    {
        // I had a clean pattern matching impl, but the jit didn't get rid of bounds checks for span slicing :(
        ref var numbersRef = ref MemoryMarshal.GetReference(numbers);
        while(i >= 1)
        {
            var last = Unsafe.Add(ref numbersRef, i);
            i--;

            if (ulong.DivRem(target, last) is (var q2, 0) && TryPart1(q2, numbers, i)) return true;

            target -= last;
        }
        
        return numbersRef == target;
    }
    
    protected override object Part1Impl()
    {
        Span<ulong> buffer = new ulong[12];
        ulong sum = 0;

        foreach (var (num, numbers) in Parse(Input.TextU8, buffer))
        {
            sum += TryPart1(num, numbers, numbers.Length - 1) ? num : 0;
        }

        return sum; // 3351424677624
    }

    private static bool TryPart2(ulong target, Span<ulong> numbers, int i)
    {
        ref var numbersRef = ref MemoryMarshal.GetReference(numbers);
        while(i >= 1)
        {
            var last = Unsafe.Add(ref numbersRef, i);
            i--;

            if (ulong.DivRem(target - last, last.NextPowerOf10()) is (var q, 0) && TryPart2(q, numbers, i)) return true;
            if (ulong.DivRem(target, last) is (var q2, 0) && TryPart2(q2, numbers, i)) return true;

            target -= last;
        }
        
        return numbersRef == target;
    }

    protected override object Part2Impl()
    {
        Span<ulong> buffer = new ulong[12];
        ulong sum = 0;

        foreach (var (num, numbers) in Parse(Input.TextU8, buffer))
        {
            sum += TryPart2(num, numbers, numbers.Length - 1) ? num : 0;
        }

        return sum; // 204976636995111
    }
}