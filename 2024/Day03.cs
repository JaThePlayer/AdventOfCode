using System.Text.RegularExpressions;

namespace AoC._2024;

/*
| Method | Mean     | Error    | StdDev   | Gen0    | Gen1    | Allocated |
|------- |---------:|---------:|---------:|--------:|--------:|----------:|
| Part1  | 98.79 us | 1.884 us | 1.573 us | 44.4336 | 24.2920 |  363.8 KB |
| Part2  | 92.20 us | 1.026 us | 0.959 us | 38.4521 | 19.1650 | 313.84 KB |

Ideas:
- Seperate regex that only searches for 'do()' when disabled.
- No regex
*/
public partial class Day03 : AdventBase
{
    public override int Year => 2024;
    public override int Day => 3;

    [GeneratedRegex(@"mul\((\d*?),(\d*?)\)")]
    private static partial Regex MulRegex();
    
    
    [GeneratedRegex(@"mul\((\d*?),(\d*?)\)|do\(\)|don't\(\)")]
    private static partial Regex MulRegexPart2();
    
    protected override object Part1Impl()
    {
        return MulRegex()
            .Matches(Input.Text)
            .Sum(m => Util.FastParseInt<int>(m.Groups[1].ValueSpan) * Util.FastParseInt<int>(m.Groups[2].ValueSpan)); // 166630675
    }

    protected override object Part2Impl()
    {
        var sum = 0;
        var enabled = true;
        foreach (var m in MulRegexPart2().Matches(Input.Text).OfType<Match>())
        {
            switch (m.ValueSpan)
            {
                case "do()":
                    enabled = true;
                    continue;
                case "don't()":
                    enabled = false;
                    continue;
            }
            if (enabled)
                sum += Util.FastParseInt<int>(m.Groups[1].ValueSpan) * Util.FastParseInt<int>(m.Groups[2].ValueSpan);
        }
        return sum; // 93465710
    }
}