using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;

namespace AoC._2025;

/*
| Method | Mean      | Error    | StdDev   | Gen0    | Gen1    | Gen2    | Allocated |
|------- |----------:|---------:|---------:|--------:|--------:|--------:|----------:|
| Part1  |  22.57 us | 0.130 us | 0.115 us |  2.9297 |  0.2136 |       - |  24.09 KB |
| Part2  | 258.78 us | 5.008 us | 4.684 us | 90.8203 | 90.8203 | 90.8203 |  440.9 KB |
 */
public class Day07 : AdventBase
{
    public override int Year => 2025;
    public override int Day => 07;
    
    protected override object Part1Impl()
    {
        var map = Input.Create2DMutMap();
        ReadOnlySpan2D<byte> rmap = map;
        
        var (sx, sy) = rmap.IndexOf2D((byte)'S');
        List<(int y, int x)> beams = [ (sy, sx)];
        List<(int y, int x)> newBeams = [];

        var splits = 0;
        
        while (beams.Count > 0)
        {
            newBeams.Clear();
            var beamSpan = CollectionsMarshal.AsSpan(beams);
            foreach (var (cy, cx) in beamSpan)
            {
                var ny = cy + 1;
                if (ny >= map.Height)
                    break;

                if (map[ny, cx] == '^')
                {
                    var anyChange = false;
                    if (cx > 0 && map[ny, cx - 1] == '.')
                    {
                        newBeams.Add((ny, cx - 1));
                        map[ny, cx - 1] = (byte)'|';
                        anyChange = true;
                    }
                    if (map[ny, cx + 1] == '.')
                    {
                        newBeams.Add((ny, cx + 1));
                        map[ny, cx + 1] = (byte)'|';
                        anyChange = true;
                    }
                    
                    if (anyChange)
                        splits++;
                }
                else
                {
                    newBeams.Add((ny, cx));
                }
            }
            
            beams.Clear();
            beams.AddRange(newBeams);
        }

        return splits; // 1550
    }

    protected override object Part2Impl()
    {
        var map = Input.Create2DMap();
        
        var (sx, sy) = map.IndexOf2D((byte)'S');

        Dictionary<(int y, int x), long> cache = [];

        return TravelFrom(map, sy, sx); // 9897897326778

        long TravelFrom(ReadOnlySpan2D<byte> map, int y, int x)
        {
            if (cache.TryGetValue((y, x), out var value))
                return value;
            
            if (y >= map.Height)
                return 1;

            if (x < 0 || x >= map.Width - 1)
                return 0;
            
            var c = map[y, x];
            if (c == '^')
            {
                var left = cache[(y + 1, x - 1)] = TravelFrom(map, y + 1, x - 1);
                var right = cache[(y + 1, x + 1)] = TravelFrom(map, y + 1, x + 1);
                return cache[(y + 1, x)] = left + right;
            }

            return cache[(y + 1, x)] = TravelFrom(map, y + 1, x);
        }
    }
}