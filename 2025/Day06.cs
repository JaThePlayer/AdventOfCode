namespace AoC._2025;
/*
Initial
| Method | Mean     | Error    | StdDev   | Gen0   | Gen1   | Allocated |
|------- |---------:|---------:|---------:|-------:|-------:|----------:|
| Part1  | 62.74 us | 0.808 us | 0.675 us | 7.9346 | 0.9766 |   66632 B |
| Part2  | 29.28 us | 0.335 us | 0.314 us |      - |      - |      24 B |
*/
public class Day06 : AdventBase
{
    public override int Year => 2025;
    public override int Day => 6;
    
    protected override object Part1Impl()
    {
        var input = Input.TextU8;
        long sum = 0;
        List<List<long>> lines = [];
        foreach (var lineRange in input.Split((byte)'\n'))
        {
            var line = input[lineRange];
            List<long> thisLine = [];
            var i = 0;
            foreach (var numRange in line.Split((byte)' '))
            {
                var num = line[numRange];
                if (num.IsEmpty)
                    continue;

                switch (num[0])
                {
                    case (byte)'*':
                    {
                        long res = 1;   
                        foreach (var parsedLine in lines)
                        {
                            res *= parsedLine[i];
                        }

                        sum += res;
                        break;
                    }
                    case (byte)'+':
                    {
                        long res = 0;   
                        foreach (var parsedLine in lines)
                        {
                            res += parsedLine[i];
                        }

                        sum += res;
                        break;
                    }
                    default:
                        thisLine.Add(Util.FastParseInt<long>(num));
                        break;
                }

                i++;
            }
            lines.Add(thisLine);
        }

        return sum; // 5346286649122
    }

    protected override object Part2Impl()
    {
        var map = Input.Create2DMap();
        long sum = 0;

        var lastLine = map.GetRowSpan(map.Height - 1);
        
        foreach (var opRange in lastLine.Split((byte)' '))
        {
            var opSpan = lastLine[opRange];
            if (opSpan.IsEmpty)
                continue;
            
            var x = opRange.Start.Value;
            var op = opSpan[0];
            
            long res = op == '*' ? 1 : 0;
            do
            {
                long num = 0;
                for (var y = 0; y < map.Height - 1; y++)
                {
                    var digit = map[y, x];
                    if (digit < '0' || digit > '9')
                        continue;
                    num = num * 10 + (digit - '0');
                }

                if (op == '*' && num != 0)
                    res *= num;
                else
                    res += num;
            }
            while (++x < lastLine.Length && lastLine[x] == (byte)' ');
            
            sum += res;
        }
        
        return sum; // 10389131401929
    }
}