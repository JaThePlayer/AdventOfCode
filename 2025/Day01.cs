namespace AoC._2025;

/*
| Method | Mean     | Error    | StdDev   | Allocated |
|------- |---------:|---------:|---------:|----------:|
| Part1  | 26.83 us | 0.167 us | 0.156 us |      24 B |
| Part2  | 27.34 us | 0.160 us | 0.150 us |      24 B |
 */
public class Day01 : AdventBase
{
    public override int Year => 2025;
    public override int Day => 1;
    
    protected override object Part1Impl()
    {
        var res = 0;
        var curr = 50;
        var input = Input.Text.AsSpan();
        foreach (var lineRange in input.Split('\n'))
        {
            var line = input[lineRange];
            var amt = Util.FastParseInt<int>(line[1..]);
            
            curr += line[0] == 'L' ? -amt : amt;

            if (curr % 100 == 0)
                res++;
        }

        return res; // 964
    }

    protected override object Part2Impl()
    {
        var res = 0;
        var curr = 50;
        var input = Input.Text.AsSpan();
        foreach (var lineRange in input.Split('\n'))
        {
            var line = input[lineRange];
            var amt = Util.FastParseInt<int>(line[1..]);

            // Simplify rotations > 100
            res += amt / 100;
            amt %= 100;

            if (line[0] == 'L')
            {
                if (curr == 0)
                {
                    // We're moving from 0, this doesn't count again
                    curr = 100 - amt;
                    continue;
                }
                
                curr -= amt;
                
                if (curr <= 0)
                {
                    // count landing on 0 or less, but only wraparound on negatives.
                    res++;
                    if (curr < 0)
                        curr += 100;
                }
            }
            else
            {
                curr += amt;
                if (curr >= 100)
                {
                    res++;
                    curr -= 100;
                }
            }
        }

        return res; // 5872
    }
}