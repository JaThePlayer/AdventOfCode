using System.Numerics;
using System.Runtime.InteropServices;

namespace AoC._2025;

/*
 Initial
| Method | Mean     | Error    | StdDev   | Gen0     | Gen1     | Gen2     | Allocated |
|------- |---------:|---------:|---------:|---------:|---------:|---------:|----------:|
| Part1  | 65.38 ms | 0.212 ms | 0.198 ms | 875.0000 | 875.0000 | 875.0000 |  15.48 MB |
| Part2  | 65.77 ms | 0.189 ms | 0.147 ms | 875.0000 | 875.0000 | 875.0000 |  15.47 MB |
 */
public class Day08 : AdventBase
{
    public override int Year => 2025;
    public override int Day => 8;
    
    record struct Box(Vector3 pos, int circuitId);

    private static IEnumerable<(int, int)> PermutationsOfIds<T>(List<T> items)
    {
        for (var i = 0; i < items.Count; i++)
            for (var j = i + 1; j < items.Count; j++)
                if (i != j)
                    yield return (i, j);
    }
    
    protected override object Part1Impl()
    {
        var input = Input.TextU8;
        List<Box> boxesList = [];
        Span<int> numberBuffer = stackalloc int[3];
        
        var currCircuitId = 0;
        foreach (var lineRange in input.Split((byte)'\n'))
        {
            var line = input[lineRange];

            Util.FastParseIntList(line, (byte)',', numberBuffer);
            boxesList.Add(new(new(numberBuffer[0], numberBuffer[1], numberBuffer[2]), currCircuitId++));
        }
        
        var boxes = CollectionsMarshal.AsSpan(boxesList);
        Span<int> circuitLengths = stackalloc int[boxes.Length];
        circuitLengths.Fill(1);

        var dists = PermutationsOfIds(boxesList)
            .Select(x => (x.Item1, x.Item2, Vector3.Distance(boxesList[x.Item1].pos, boxesList[x.Item2].pos)))
            .OrderBy(x => x.Item3)
            .GetEnumerator();
        
        var steps = 0;
        while (steps < (boxesList.Count < 30 ? 10 : 1000))
        {
            steps++;
            dists.MoveNext();
            var (i, j, dist) = dists.Current;
            if (boxes[i].circuitId == boxes[j].circuitId)
            {
                continue;
            }

            var a = boxes[i].circuitId;
            var b = boxes[j].circuitId;
            var lenA = circuitLengths[a];
            var lenB = circuitLengths[b];

            if (lenA > lenB)
            {
                circuitLengths[a] += lenB;
                circuitLengths[b] = 0;
                foreach (ref var box in boxes)
                    if (box.circuitId == b)
                        box.circuitId = a;
            }
            else
            {
                circuitLengths[a] = 0;
                circuitLengths[b] += lenA;
                foreach (ref var box in boxes)
                    if (box.circuitId == a)
                        box.circuitId = b;
            }
        }
        return circuitLengths.ToArray().OrderDescending().Take(3).Aggregate(1, (a, b) => a * b); // 131150
    }

    protected override object Part2Impl()
    {
        var input = Input.TextU8;
        List<Box> boxesList = [];
        Span<int> numberBuffer = stackalloc int[3];
        
        var currCircuitId = 0;
        foreach (var lineRange in input.Split((byte)'\n'))
        {
            var line = input[lineRange];
            Util.FastParseIntList(line, (byte)',', numberBuffer);
            boxesList.Add(new(new(numberBuffer[0], numberBuffer[1], numberBuffer[2]), currCircuitId++));
        }
        
        var boxes = CollectionsMarshal.AsSpan(boxesList);
        Span<int> circuitLengths = stackalloc int[boxes.Length];
        circuitLengths.Fill(1);

        var dists = PermutationsOfIds(boxesList)
            .Select(x => (x.Item1, x.Item2, Vector3.Distance(boxesList[x.Item1].pos, boxesList[x.Item2].pos)))
            .OrderBy(x => x.Item3)
            .GetEnumerator();

        while (dists.MoveNext())
        {
            var (i, j, dist) = dists.Current;
            if (boxes[i].circuitId == boxes[j].circuitId)
                continue;

            var a = boxes[i].circuitId;
            var b = boxes[j].circuitId;
            var lenA = circuitLengths[a];
            var lenB = circuitLengths[b];

            if (lenA > lenB)
            {
                circuitLengths[a] += lenB;
                circuitLengths[b] = 0;
                foreach (ref var box in boxes)
                    if (box.circuitId == b)
                        box.circuitId = a;
            }
            else
            {
                circuitLengths[a] = 0;
                circuitLengths[b] += lenA;
                foreach (ref var box in boxes)
                    if (box.circuitId == a)
                        box.circuitId = b;
            }

            if (circuitLengths[a] == boxes.Length || circuitLengths[b] == boxes.Length)
                return boxes[i].pos.X * boxes[j].pos.X;
        }

        throw new Exception("OOPS");
    }
}