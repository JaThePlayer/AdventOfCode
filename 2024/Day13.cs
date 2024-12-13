namespace AoC._2024;

/*
| Method | Mean     | Error    | StdDev   | Allocated |
|------- |---------:|---------:|---------:|----------:|
| Part1  | 11.12 us | 0.063 us | 0.053 us |      24 B |
| Part2  | 12.12 us | 0.108 us | 0.101 us |      24 B |
 */
public class Day13 : AdventBase
{
    public override int Year => 2024;
    public override int Day => 13;
    
    static void ParseButtonLine(ReadOnlySpan<char> line, out int x, out int y) {
        (x, y) = (
            Util.FastParseInt<int>(line["Button A: X+".Length.. ("Button A: X+".Length + 2)]),
            Util.FastParseInt<int>(line["Button A: X+26, Y+".Length.. ("Button A: X+26, Y+".Length + 2)])
        );
    }
    
    static void ParsePrizeLine(ReadOnlySpan<char> line, out long x, out long y)
    {
        line = line["Prize: X=".Length..];
        var splitIdx = line.IndexOf(',');
        x = Util.FastParseInt<long>(line[..splitIdx]);
        y = Util.FastParseInt<long>(line[(splitIdx+4)..]);
    }

    static long FindSolution(int ax, int ay, int bx, int by, long px, long py)
    {
        // Cramer's rule
        var detA = ax * by - bx * ay;
        var detAa = px * by - bx * py;
        var detAb = ax * py - px * ay;
        var a = detAa / detA;
        var b = detAb / detA;
        
        // See if the solution is valid
        if (ax*a + bx*b == px && ay*a + by*b == py)
            return 3*a + b;
        return 0;
    }
    
    protected override object Part1Impl()
    {
        var input = Input.Text.AsSpan();

        long sum = 0;

        var enumerable = input.Split('\n');
        while (enumerable.MoveNext())
        {
            var lineA = input[enumerable.Current];
            enumerable.MoveNext();
            var lineB = input[enumerable.Current];
            enumerable.MoveNext();
            var linePrize = input[enumerable.Current];
            enumerable.MoveNext();

            ParseButtonLine(lineA, out var ax, out var ay);
            ParseButtonLine(lineB, out var bx, out var by);
            ParsePrizeLine(linePrize, out var px, out var py);

            sum += (long)FindSolution(ax, ay, bx, by, px, py);
        }

        return sum; // 32026
    }

    protected override object Part2Impl()
    {
        var input = Input.Text.AsSpan();

        long sum = 0;

        var enumerable = input.Split('\n');
        while (enumerable.MoveNext())
        {
            var lineA = input[enumerable.Current];
            enumerable.MoveNext();
            var lineB = input[enumerable.Current];
            enumerable.MoveNext();
            var linePrize = input[enumerable.Current];
            enumerable.MoveNext();

            ParseButtonLine(lineA, out var ax, out var ay);
            ParseButtonLine(lineB, out var bx, out var by);
            ParsePrizeLine(linePrize, out var px, out var py);
            px += 10000000000000;
            py += 10000000000000;

            sum += (long)FindSolution(ax, ay, bx, by, px, py);
        }

        return sum;
    }
}