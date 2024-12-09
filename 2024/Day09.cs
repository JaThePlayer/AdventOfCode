using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;

namespace AoC._2024;

/*
 
Initial
| Method | Mean        | Error    | StdDev   | Gen0     | Gen1     | Gen2     | Allocated  |
|------- |------------:|---------:|---------:|---------:|---------:|---------:|-----------:|
| Part1  |    614.0 us |  2.38 us |  1.99 us | 285.1563 | 285.1563 | 285.1563 | 1618.04 KB |
| Part2  | 11,570.4 us | 31.95 us | 26.68 us | 171.8750 | 171.8750 | 171.8750 |  768.66 KB |

Part2: Trim empty 'empties' entries
| Method | Mean       | Error    | StdDev   | Gen0     | Gen1     | Gen2     | Allocated  |
|------- |-----------:|---------:|---------:|---------:|---------:|---------:|-----------:|
| Part1  |   619.1 us |  3.17 us |  2.97 us | 285.1563 | 285.1563 | 285.1563 | 1618.04 KB |
| Part2  | 6,566.5 us | 12.68 us | 11.24 us | 179.6875 | 179.6875 | 179.6875 |  768.66 KB |

Part2:  Don't add empty regions to 'empties'
| Method | Mean     | Error     | StdDev    | Gen0     | Gen1     | Gen2     | Allocated |
|------- |---------:|----------:|----------:|---------:|---------:|---------:|----------:|
| Part2  | 5.217 ms | 0.0316 ms | 0.0264 ms | 179.6875 | 179.6875 | 179.6875 | 768.66 KB |

Part 2: Remove 'Index' from region, just use i instead.
| Method | Mean     | Error     | StdDev    | Gen0    | Gen1    | Gen2    | Allocated |
|------- |---------:|----------:|----------:|--------:|--------:|--------:|----------:|
| Part2  | 3.467 ms | 0.0075 ms | 0.0062 ms | 82.0313 | 82.0313 | 82.0313 | 512.66 KB |

Part 2: fastpath: startJ instead of removing first empty region
| Method | Mean     | Error     | StdDev    | Gen0    | Gen1    | Gen2    | Allocated |
|------- |---------:|----------:|----------:|--------:|--------:|--------:|----------:|
| Part2  | 3.336 ms | 0.0072 ms | 0.0060 ms | 82.0313 | 82.0313 | 82.0313 | 512.66 KB |

Part 2: O(1) file hashing algo
| Method | Mean     | Error     | StdDev    | Gen0    | Gen1    | Gen2    | Allocated |
|------- |---------:|----------:|----------:|--------:|--------:|--------:|----------:|
| Part2  | 3.058 ms | 0.0073 ms | 0.0057 ms | 82.0313 | 82.0313 | 82.0313 | 512.66 KB |

Part 2: Presizing and stackallocing regions
| Method | Mean     | Error     | StdDev    | Gen0   | Allocated |
|------- |---------:|----------:|----------:|-------:|----------:|
| Part2  | 2.999 ms | 0.0094 ms | 0.0079 ms | 7.8125 |  78.21 KB |

Part 2 rewrite
| Method | Mean     | Error   | StdDev  | Gen0    | Gen1   | Allocated |
|------- |---------:|--------:|--------:|--------:|-------:|----------:|
| Part2  | 351.3 us | 1.10 us | 0.92 us | 11.7188 | 1.4648 |  98.23 KB |
 */
public class Day09 : AdventBase
{
    public override int Year => 2024;
    public override int Day => 9;
    
    protected override object Part1Impl()
    {
        long sum = 0;
        var input = Input.Text.AsSpan();
        var files = new List<int>();
        var nextFileId = 0;
        
        for (int i = 0; i < input.Length; i += 2)
        {
            var c = input[i] - '0';
            files.AddRange(Enumerable.Repeat(nextFileId, c));
            if (i + 1 < input.Length)
            {
                var free = input[i+1] - '0';
                files.AddRange(Enumerable.Repeat(-1, free));
            }

            nextFileId++;
        }
        
        var l = 0;
        var r = files.Count - 1;
        var f = CollectionsMarshal.AsSpan(files);
        while (l < r && r > 0)
        {
            while (f[l] != -1 && l+1 < r)
                l++;
            while (f[r] == -1 && r > 0)
                r--;

            if (l < r)
            {
                f[l] = f[r];
                f[r] = -1;
                l++;
                r--;
            }
        }

        for (int i = 0; i < f.Length && f[i] >= 0; i++)
        {
            sum += f[i] * i;
        }

        return sum;
    }

    record struct Region(int Index, int Length);
    
    protected override object Part2Impl()
    {
        long sum = 0;
        var input = Input.Text.AsSpan();

        Span<Region> files = stackalloc Region[input.Length / 2 + 1];
        var fileI = 0;
        var empties = new List<int>[10];
        var idx = 0;
        
        for (var i = 0; i < input.Length; i += 2)
        {
            var c = input[i] - '0';
            files[fileI++] = new Region(idx, c);
            idx += c;

            if ((uint)(i + 1) >= input.Length)
                continue;
            
            var free = input[i + 1] - '0';
            if (free == 0)
                continue;
            
            ref var empt = ref empties.DangerousGetReferenceAt(free);
            empt ??= new();
                    
            empt.Add(idx);
            idx += free;
        }
        
        for (var i = files.Length - 1; i >= 0; i--)
        {
            ref var file = ref files[i];
            var len = file.Length;

            var mostLeftRegion = new Region(99999, 9999);

            // Find the first empty region large enough for our file
            for (int j = len; j < 10; j++)
            {
                var list = empties[j];
                if (list.Count <= 0)
                    continue;
                // Because the lists are sorted, the leftmost region is at index 0.
                var firstEmpty = list[0];
                if (firstEmpty > file.Index)
                    continue;
                if (firstEmpty < mostLeftRegion.Index)
                    mostLeftRegion = new(firstEmpty, j);
            }

            // We found an empty region
            if (mostLeftRegion.Length < 9999)
            {
                file.Index = mostLeftRegion.Index;
                
                empties[mostLeftRegion.Length].RemoveAt(0);
                mostLeftRegion.Length -= len;
                mostLeftRegion.Index += len;

                if (mostLeftRegion.Length > 0)
                {
                    var list = empties[mostLeftRegion.Length];
                    for (var x = 0; x < list.Count; x++)
                    {
                        if (list[x] < mostLeftRegion.Index)
                            continue;
                        list.Insert(x,mostLeftRegion.Index );
                        break;
                    }
                }
            }
            
            sum +=(long)(i * file.Length * (file.Index + (file.Length - 1) / 2.0) );
        }
        
        //if (sum != 6221662795602)
        //    throw new Exception(sum.ToString());

        return sum; // 6221662795602
    }
}