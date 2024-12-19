using System.Runtime.InteropServices;
using System.Text;
using CommunityToolkit.HighPerformance;

namespace AoC._2024;

/*
| Method | Mean      | Error     | StdDev    | Gen0     | Gen1     | Gen2     | Allocated  |
|------- |----------:|----------:|----------:|---------:|---------:|---------:|-----------:|
| Part1  |  3.457 ms | 0.0259 ms | 0.0216 ms |        - |        - |        - |   22.18 KB |
| Part2  | 16.396 ms | 0.0943 ms | 0.0836 ms | 437.5000 | 312.5000 | 218.7500 | 3614.22 KB |

P2: Alternate lookup
| Part2  | 14.98 ms | 0.116 ms | 0.103 ms | 218.7500 | 218.7500 | 218.7500 |   1.61 MB |

Invert sort order
| Method | Mean        | Error     | StdDev   | Gen0     | Gen1     | Gen2     | Allocated  |
|------- |------------:|----------:|---------:|---------:|---------:|---------:|-----------:|
| Part1  |    569.1 us |   1.09 us |  0.91 us |   1.9531 |        - |        - |   22.18 KB |
| Part2  | 15,075.9 us | 108.41 us | 96.10 us | 218.7500 | 218.7500 | 218.7500 | 1652.56 KB |

Different lists per last char
| Method | Mean       | Error    | StdDev   | Gen0     | Gen1     | Gen2     | Allocated  |
|------- |-----------:|---------:|---------:|---------:|---------:|---------:|-----------:|
| Part1  |   207.5 us |  1.61 us |  1.43 us |   2.9297 |        - |        - |   25.67 KB |
| Part2  | 6,273.3 us | 26.98 us | 21.07 us | 218.7500 | 218.7500 | 218.7500 | 1656.05 KB |

P2 sorting
| Part2  | 5.511 ms | 0.0134 ms | 0.0119 ms | 218.7500 | 218.7500 | 218.7500 |   1.62 MB |

P2 clear memo each towel
| Part2  | 5.051 ms | 0.0106 ms | 0.0094 ms | 85.9375 |  764.6 KB |

Early return thanks to sorting
| Method | Mean       | Error    | StdDev   | Gen0    | Allocated |
|------- |-----------:|---------:|---------:|--------:|----------:|
| Part1  |   195.2 us |  1.78 us |  1.67 us |  2.9297 |  25.67 KB |
| Part2  | 4,923.4 us | 24.34 us | 20.32 us | 85.9375 |    756 KB |

P2 - memoise with length instead of array
| Method | Mean       | Error    | StdDev   | Gen0   | Allocated |
|------- |-----------:|---------:|---------:|-------:|----------:|
| Part1  |   201.8 us |  2.43 us |  2.16 us | 2.9297 |  25.67 KB |
| Part2  | 3,458.5 us | 30.87 us | 28.87 us |      - |  30.34 KB |

| Method | Mean       | Error    | StdDev   | Gen0   | Allocated |
|------- |-----------:|---------:|---------:|-------:|----------:|
| Part1  |   198.4 us |  2.62 us |  2.45 us | 2.9297 |  25.67 KB |
| Part2  | 3,339.4 us | 15.75 us | 14.74 us |      - |  26.84 KB |
 */
public class Day19 : AdventBase
{
    public override int Year => 2024;
    public override int Day => 19;
    
    protected override object Part1Impl()
    {
        var input = Input.TextU8;
        var lines = input.Split((byte)'\n');
        lines.MoveNext();

        var towels = new List<byte[]>?['z'];
        foreach (var n in input[lines.Current].ParseSplits((byte)',', 0, (span, _) => span.TrimStart((byte)' ').ToArray()))
        {
            ref var col = ref towels[n[^1]];
            col ??= new();
            col.Add(n);
        }
        foreach (var t in towels)
        {
            t?.Sort((x1, x2) => x1.Length.CompareTo(x2.Length));
        }
        

        lines.MoveNext(); // skip empty line

        var sum = 0;
        while (lines.MoveNext())
        {
            var line = input[lines.Current];
            sum += CheckTowel(line, towels) ? 1 : 0;
        }

        return sum; // 280

        bool CheckTowel(ReadOnlySpan<byte> towel, List<byte[]>?[] towels)
        {
            if (towel.Length == 0)
                return true;
            
            foreach (var avail in towels[towel[^1]]!)
            {                if (avail.Length > towel.Length)
                    break;
                if (towel.EndsWith(avail))
                {
                    if (CheckTowel(towel[..(^avail.Length)], towels))
                        return true;
                }
            }

            return false;
        }
    }

    protected override object Part2Impl()
    {
        var input = Input.TextU8;
        var lines = input.Split((byte)'\n');
        lines.MoveNext();
        
        var towels = new List<byte[]>?['z'];
        foreach (var n in input[lines.Current].ParseSplits((byte)',', 0, (span, _) => span.TrimStart((byte)' ').ToArray()))
        {
            ref var col = ref towels[n[^1]];
            col ??= new();
            col.Add(n);
        }
        foreach (var t in towels)
        {
            t?.Sort((x1, x2) => x1.Length.CompareTo(x2.Length));
        }

        var singleTowelsPossible = new byte['z'];
        singleTowelsPossible['w'] = towels['w']![0].Length is 1 ? (byte)1 : (byte)0;
        singleTowelsPossible['u'] = towels['u']![0].Length is 1 ? (byte)1 : (byte)0;
        singleTowelsPossible['b'] = towels['b']![0].Length is 1 ? (byte)1 : (byte)0;
        singleTowelsPossible['r'] = towels['r']![0].Length is 1 ? (byte)1 : (byte)0;
        singleTowelsPossible['g'] = towels['g']![0].Length is 1 ? (byte)1 : (byte)0;

        lines.MoveNext(); // skip empty line
        
        var memo = new long[64];
        memo[0] = 1;
        var clearableMemo = memo.AsSpan(2);
        
        long sum = 0;
        while (lines.MoveNext())
        {
            var line = input[lines.Current];
            clearableMemo.Fill(long.MaxValue);

            memo[1] = singleTowelsPossible.DangerousGetReferenceAt(line[0]);
            sum += CheckTowel(line);
        }

        return sum; // 606411968721181
        

        long CheckTowel(ReadOnlySpan<byte> towel)
        {
            ref var m = ref memo[towel.Length];
            if (m != long.MaxValue)
                return m;
            
            long sum = 0;
            foreach (var avail in CollectionsMarshal.AsSpan(towels[towel[^1]]!))
            {
                if (avail.Length > towel.Length)
                    break;
                if (towel.EndsWith(avail))
                    sum += CheckTowel(towel[..^avail.Length]);
            }

            m = sum;
            return sum;
        }
    }
}