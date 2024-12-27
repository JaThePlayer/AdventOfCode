namespace AoC._2024;


/*
Initial - 12s for p2
 
Optimised Bruteforce
| Method | Mean    | Error    | StdDev   | Gen0      | Allocated |
|------- |--------:|---------:|---------:|----------:|----------:|
| Part2  | 3.457 s | 0.0198 s | 0.0185 s | 1000.0000 |  15.13 MB |
 */

public class Day22 : AdventBase
{
    public override int Year => 2024;
    public override int Day => 22;

    private static ulong Next(ulong v)
    {
        var prev = v;
        v = prune(mix(v, v << 6));
        v = mix(v, v >> 5);
        v = prune(mix(v, v << 11));

       // Console.WriteLine((prev, v, $"{prev:b32} - {v:b32}", v % 10));
        
        ulong mix(ulong secret, ulong n)
        {
            return n ^ secret;
        }

        ulong prune(ulong secret)
        {
            return secret % 16777216;
        }

        return v;
    }
    
    private ulong NaiveCalc(ulong initial)
    {
        ulong v = initial;

        for (ulong i = 0; i < 2000; i++)
        {
            v = Next(v);
        }
        
        return v;
    }
    
    private (sbyte[], int[]) Memoize(ulong initial)
    {
        sbyte[] memo = new sbyte[2000];
        int[] costs = new int[2000];
        ulong v = initial;
        ulong prev = initial % 10;

        for (ulong i = 0; i < 2000; i++)
        {
            v = Next(v);
            memo[i] = (sbyte)((v % 10) - prev);
            costs[i] = (int)(v % 10);
            prev = v % 10;

           // Console.WriteLine($"{v}: {v % 10} ({memo[i]})");
        }
        
        return (memo, costs);
    }

    protected override object Part1Impl()
    {
        ulong sum = 0;

        foreach (var initial in Input.TextU8.ParseSplits((byte)'\n', 0, (span, _) => Util.FastParseInt<ulong>(span)))
        {
            var res = NaiveCalc(initial);
            sum += res;
            //Console.WriteLine((initial, res));
        }
        
        return sum; // 13022553808
    }

    protected override object Part2Impl()
    {
        var input = ParseInputP2();
        
        // Input.TextU8.ParseSplits((byte)'\n', 0, (span, _) => Util.FastParseInt<ulong>(span)).ToList();

        ulong bestSum = 0;
        ulong checkedAmt = 0;

        int[] firstIndexesOfDouble = new int[input.Count];
        int[] firstIndexesOfTriple = new int[input.Count];

        for (int a = -9; a <= 9; a++)
        {
            //Console.WriteLine(a);
            for (int b = int.Max(-9 - a, -9); b <= int.Min(9 - a, 9); b++)
            {
                ReadOnlySpan<sbyte> search2 = [(sbyte)a, (sbyte)b];
                for (int i = 0; i < firstIndexesOfDouble.Length; i++)
                {
                    firstIndexesOfDouble[i] = input[i].Item1.AsSpan().IndexOf(search2);
                }

                for (int c = int.Max(-9 - a - b, -9); c <= int.Min(9 - b, 9); c++)
                {
                    // saves 5s + 1.4s when used in Check
                    ReadOnlySpan<sbyte> search = [(sbyte)a, (sbyte)b, (sbyte)c];
                    for (int i = 0; i < firstIndexesOfTriple.Length; i++)
                    {
                        var si = firstIndexesOfDouble[i];
                        if (si >= 0)
                        {
                            var idx = input[i].Item1.AsSpan(si).IndexOf(search);
                            firstIndexesOfTriple[i] = idx < 0 ? idx : idx + si;
                        }
                    }
                    
                    for (int d = int.Max(-9 - a - b - c, -9); d <= int.Min(9 -  c, 9); d++)
                    {
                        var sum = Check((sbyte)a, (sbyte)b, (sbyte)c, (sbyte)d, input, bestSum, firstIndexesOfTriple);
                        if (sum > bestSum)
                        {
                           // Console.WriteLine($"FOUND NEW BEST SUM: {sum} vs {bestSum}: {(a, b, c, d)}");
                        }
                        bestSum = ulong.Max(bestSum, sum);
                        checkedAmt++;
                    }
                }

            }

        }


        Console.WriteLine(checkedAmt); // 59221
            
        return bestSum; // 1555 for (-1, 2, 0, 0)
    }

    private List<(sbyte[], int[])> ParseInputP2()
    {
        return Input.TextU8
            .SplitSlim((byte)'\n')
            .Select(x => Memoize(Util.FastParseInt<ulong>(x)))
            .ToList(default((sbyte[], int[])));
    }

    private static ulong Check(sbyte a, sbyte b, sbyte c, sbyte d, List<(sbyte[], int[])> input, ulong curBest, int[] startSearchIndexes)
    {
        ReadOnlySpan<sbyte> search = [a, b, c, d];
        ulong sum = 0UL;
        for (var i = 0; i < input.Count; i++)
        {
            if (startSearchIndexes[i] < 0)
            {
                continue;
            }
            
            var (memo, costs) = input[i];
            var index = memo.AsSpan(startSearchIndexes[i]).IndexOf(search);
            if (index >= 0)
            {
                sum += (ulong)costs[index+ startSearchIndexes[i] + 3];
            }

            // saves like 1s, not needed
            if (sum + (ulong)(input.Count - i - 1) * 5 < curBest)
            {
                return ulong.MinValue;
            }
        }

        return sum;
    }
}