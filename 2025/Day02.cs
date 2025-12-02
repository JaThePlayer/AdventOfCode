namespace AoC._2025;

/*
 Initial:
| Method | Mean      | Error    | StdDev   | Gen0        | Allocated  |
|------- |----------:|---------:|---------:|------------:|-----------:|
| Part1  |  23.20 ms | 0.454 ms | 0.831 ms |  11406.2500 |   91.16 MB |
| Part2  | 431.68 ms | 8.528 ms | 9.821 ms | 188000.0000 | 1500.45 MB |

Utf8
| Method | Mean      | Error    | StdDev   | Gen0        | Allocated  |
|------- |----------:|---------:|---------:|------------:|-----------:|
| Part1  |  23.09 ms | 0.452 ms | 0.791 ms |  11406.2500 |   91.16 MB |
| Part2  | 464.75 ms | 8.777 ms | 9.014 ms | 188000.0000 | 1500.45 MB |


Part 1: Do everything on numbers and not strings
| Method | Mean     | Error     | StdDev    | Allocated |
|------- |---------:|----------:|----------:|----------:|
| Part1  | 6.005 ms | 0.0183 ms | 0.0171 ms |         - |

Part 1: Skip ahead on odd number of digits
| Method | Mean     | Error     | StdDev    | Allocated |
|------- |---------:|----------:|----------:|----------:|
| Part1  | 5.352 ms | 0.0041 ms | 0.0036 ms |         - |

Part 1: Skip ahead on successful find
| Method | Mean     | Error   | StdDev  | Allocated |
|------- |---------:|--------:|--------:|----------:|
| Part1  | 838.4 us | 4.16 us | 3.89 us |      24 B |

Part 1: Calculate invalid ids and check whether they're in range.
| Method | Mean     | Error     | StdDev    | Allocated |
|------- |---------:|----------:|----------:|----------:|
| Part1  | 9.243 us | 0.0502 us | 0.0392 us |      24 B |

Part 1: microopts
| Method | Mean     | Error     | StdDev    | Allocated |
|------- |---------:|----------:|----------:|----------:|
| Part1  | 3.810 us | 0.0133 us | 0.0125 us |      24 B |

Part 2: better algo, no more strings
| Method | Mean      | Error     | StdDev    | Gen0   | Allocated |
|------- |----------:|----------:|----------:|-------:|----------:|
| Part1  |  3.823 us | 0.0382 us | 0.0357 us |      - |      24 B |
| Part2  | 12.862 us | 0.0441 us | 0.0391 us | 0.8698 |    7336 B |
 */
public class Day02 : AdventBase
{
    public override int Year => 2025;
    
    public override int Day => 2;
    
    protected override object Part1Impl()
    {
        var input = Input.TextU8;
        ulong res = 0;
        
        foreach (var rangeRange in input.Split((byte)','))
        {
            var range = input[rangeRange];
            _ = range.ParseTwoSplits((byte)'-', Util.FastParseInt<ulong, byte>, out var firstId, out var lastId);

            var id = firstId;
            while (true)
            {
                var digits = id.CountDigits();
                if (digits % 2 == 1)
                {
                    // Skip ahead to next power of ten
                    id = id.NextPowerOf10();
                    if (id > lastId)
                        break;
                    continue;
                }
                var pow = MathExt.PowerOfTen(digits / 2);
                var left = id / pow;
                var invalidIdForThisLeftValue = left * pow + left;

                if (firstId <= invalidIdForThisLeftValue && invalidIdForThisLeftValue <= lastId)
                    res += invalidIdForThisLeftValue;
                else if (invalidIdForThisLeftValue > lastId)
                    break; // the current invalid id was already too big, and subsequent ones will be even larger
                
                // We can't find another bad id until 'left' changes
                id += pow;
            }
        }

        return res; // 40055209690
    }

    protected override object Part2Impl()
    {
        var input = Input.TextU8;

        ulong res = 0;
        HashSet<ulong> seen = [];
        
        foreach (var rangeRange in input.Split((byte)','))
        {
            seen.Clear();
            var range = input[rangeRange];
            _ = range.ParseTwoSplits((byte)'-', Util.FastParseInt<ulong, byte>, out var firstId, out var lastId);

            for (var chunkLen = 1; chunkLen < lastId.CountDigits() / 2 + 1; chunkLen++)
            {
                var id = ulong.Max(firstId, 10);
                while (true)
                {
                    var digits = id.CountDigits();
                    
                    if (digits % chunkLen != 0)
                    {
                        // Skip ahead to next power of ten
                        id = id.NextPowerOf10();
                        if (id > lastId)
                            break;
                        continue;
                    }
                    var chunkAmt = digits / chunkLen;
                    
                    var pow = MathExt.PowerOfTen(chunkLen == 1 ? digits - 1 : digits - chunkLen);
                    var left = id / pow;
                    var target = left;
                    while (chunkAmt > 1)
                    {
                        target = target.Concat(left);
                        chunkAmt--;
                    }
                    
                    if (firstId <= target && target <= lastId)
                    {
                        if (seen.Add(target))
                        {
                            res += target;
                        }
                    }
                    else if (target > lastId)
                        break; // the current invalid id was already too big, and subsequent ones will be even larger
                    
                    // We can't find another bad id until 'left' changes
                    id += pow;
                }
            }
        }

        return res; // 50857215650
    }
}