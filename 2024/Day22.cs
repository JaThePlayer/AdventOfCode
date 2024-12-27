namespace AoC._2024;


//12.7984 s/op - P2

public class Day22 : AdventBase
{
    public override int Year => 2024;
    public override int Day => 22;

    private static ulong Next(ulong v)
    {
        v = prune(mix(v, 64UL * v));
        v = mix(v, v / 32);
        v = prune(mix(v, 2048UL * v));
        
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
        
        return sum;
    }

    protected override object Part2Impl()
    {
        var input = ParseInputP2();
        
        // Input.TextU8.ParseSplits((byte)'\n', 0, (span, _) => Util.FastParseInt<ulong>(span)).ToList();

        ulong bestSum = 0;
        //ulong checkedAmt = 0;

        for (int a = -9; a <= 9; a++)
        {
            //Console.WriteLine(a);
            for (int b = int.Max(-9 - a, -9); b <= int.Min(9 - a, 9); b++)
            for (int c = int.Max(-9 - a - b, -9); c <= int.Min(9 - b, 9); c++)
            for (int d = int.Max(-9 - a - b - c, -9); d <= int.Min(9 -  c, 9); d++)
            {
                var sum = Check((sbyte)a, (sbyte)b, (sbyte)c, (sbyte)d, input, bestSum);
                if (sum > bestSum)
                {
                    Console.WriteLine($"FOUND NEW BEST SUM: {sum} vs {bestSum}: {(a, b, c, d)}");
                }
                bestSum = ulong.Max(bestSum, sum);
                //checkedAmt++;
            }
        }


       // Console.WriteLine(checkedAmt); // 59221
        //Check(-2, 1, -1, 3, input);
            
        return bestSum; // 1555
    }

    private List<(sbyte[], int[])> ParseInputP2()
    {
        return Input.TextU8
            .SplitSlim((byte)'\n')
            .Select(x => Memoize(Util.FastParseInt<ulong>(x)))
            .ToList(default((sbyte[], int[])));
    }

    private static ulong Check(sbyte a, sbyte b, sbyte c, sbyte d, List<(sbyte[], int[])> input, ulong curBest)
    {
        ReadOnlySpan<sbyte> search = [a, b, c, d];
        ulong sum = 0UL;
        for (var i = 0; i < input.Count; i++)
        {
            var (memo, costs) = input[i];
            var index = memo.AsSpan().IndexOf(search);
            if (index >= 0)
            {
                sum += (ulong)costs[index + 3];
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