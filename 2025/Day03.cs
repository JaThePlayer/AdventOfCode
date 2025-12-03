namespace AoC._2025;

/*
Initial
| Method | Mean     | Error    | StdDev   | Allocated |
|------- |---------:|---------:|---------:|----------:|
| Part1  | 13.94 us | 0.193 us | 0.180 us |      24 B |
| Part2  | 34.06 us | 0.564 us | 0.500 us |      24 B |
 */
public class Day03 : AdventBase
{
    public override int Year => 2025;
    public override int Day => 3;
    
    protected override object Part1Impl()
    {
        var sum = 0;
        var input = Input.TextU8;
        foreach (var bankRange in input.Split((byte)'\n'))
        {
            var bank = input[bankRange];
            var tensDigitIdx = 0;
            var tensDigit = bank[0];
            for (int i = 1; i < bank.Length - 1; i++)
            {
                if (bank[i] > tensDigit)
                {
                    tensDigit = bank[i];
                    tensDigitIdx = i;
                }
            }
            
            var bat2 = bank[tensDigitIdx + 1];
            for (int i = tensDigitIdx + 2; i < bank.Length; i++)
            {
                if (bank[i] > bat2)
                {
                    bat2 = bank[i];
                }
            }

            var res = (tensDigit - '0') * 10 + (bat2 - '0');
            sum += res;
        }

        return sum; // 17196
    }

    protected override object Part2Impl()
    {
        ulong sum = 0;
        Span<byte> chars = stackalloc byte[12];
        var input = Input.TextU8;
        foreach (var bankRange in input.Split((byte)'\n'))
        {
            var bank = input[bankRange];
            var startIdx = 0;
            for (var charI = 0; charI < chars.Length; charI++)
            {
                var nextDigit = bank[startIdx];
                for (int i = startIdx + 1; i < bank.Length - (chars.Length - charI - 1); i++)
                {
                    if (bank[i] > nextDigit)
                    {
                        nextDigit = bank[i];
                        startIdx = i;
                    }
                }

                chars[charI] = nextDigit;
                startIdx++;
            }

            sum += Util.FastParseInt<ulong>(chars);
        }

        return sum; // 171039099596062
    }
}