namespace AoC._2025;

/*
Initial
| Method | Mean        | Error    | StdDev   | Gen0   | Allocated |
|------- |------------:|---------:|---------:|-------:|----------:|
| Part1  |    92.02 us | 0.574 us | 0.509 us |      - |      24 B |
| Part2  | 1,659.68 us | 4.994 us | 4.170 us | 1.9531 |   18961 B |
*/
public class Day04 : AdventBase
{
    public override int Year => 2025;
    public override int Day => 4;
    
    protected override object Part1Impl()
    {
        var map = Input.Create2DMap();
        var sum = 0;
        
        for (var y = 0; y < map.Height; y++)
        {
            for (var x = 0; x < map.Width - 1; x++)
            {
                var cell = map[y, x];
                if (cell != '@')
                    continue;

                var neigh = 0;
                if (x > 0)
                {
                    neigh += map[y, x - 1] == '@' ? 1 : 0;
                    if (y > 0)
                        neigh += map[y - 1, x - 1] == '@' ? 1 : 0;
                    if (y < map.Height - 1)
                        neigh += map[y + 1, x - 1] == '@' ? 1 : 0;
                }
                
                if (y > 0)
                    neigh += map[y - 1, x] == '@' ? 1 : 0;
                if (y < map.Height - 1)
                    neigh += map[y + 1, x] == '@' ? 1 : 0;

                if (x < map.Width - 2)
                {
                    neigh += map[y, x + 1] == '@' ? 1 : 0;
                    if (y > 0)
                        neigh += map[y - 1, x + 1] == '@' ? 1 : 0;
                    if (y < map.Height - 1)
                        neigh += map[y + 1, x + 1] == '@' ? 1 : 0;
                }

                if (neigh < 4)
                {
                    sum++;
                }
            }
        }

        return sum; // 1543
    }

    protected override object Part2Impl()
    {
        var map = Input.Create2DMutMap();
        var sum = 0;
        int changes;

        do
        {
            changes = 0;
            for (var y = 0; y < map.Height; y++)
            {
                for (var x = 0; x < map.Width - 1; x++)
                {
                    var cell = map[y, x];
                    if (cell != '@')
                        continue;

                    var neigh = 0;
                    if (x > 0)
                    {
                        neigh += map[y, x - 1] == '@' ? 1 : 0;
                        if (y > 0)
                            neigh += map[y - 1, x - 1] == '@' ? 1 : 0;
                        if (y < map.Height - 1)
                            neigh += map[y + 1, x - 1] == '@' ? 1 : 0;
                    }

                    if (y > 0)
                        neigh += map[y - 1, x] == '@' ? 1 : 0;
                    if (y < map.Height - 1)
                        neigh += map[y + 1, x] == '@' ? 1 : 0;

                    if (x < map.Width - 2)
                    {
                        neigh += map[y, x + 1] == '@' ? 1 : 0;
                        if (y > 0)
                            neigh += map[y - 1, x + 1] == '@' ? 1 : 0;
                        if (y < map.Height - 1)
                            neigh += map[y + 1, x + 1] == '@' ? 1 : 0;
                    }

                    if (neigh < 4)
                    {
                        sum++;
                        map[y, x] = (byte)'x';
                        changes++;
                    }
                }
            }
        } while (changes > 0);

        return sum; // 9038
    }
}