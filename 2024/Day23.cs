using System.Runtime.InteropServices;

namespace AoC._2024;

/*
Initial
| Method | Mean       | Error   | StdDev  | Median     | Gen0        | Gen1       | Gen2      | Allocated     |
|------- |-----------:|--------:|--------:|-----------:|------------:|-----------:|----------:|--------------:|
| Part1  |   177.7 ms | 3.28 ms | 5.10 ms |   175.1 ms |           - |          - |         - |     660.47 KB |
| Part2  | 1,575.8 ms | 6.67 ms | 5.91 ms | 1,576.4 ms | 169000.0000 | 43000.0000 | 5000.0000 | 1383345.58 KB |
*/

public class Day23 : AdventBase
{
    public override int Year => 2024;
    public override int Day => 23;
    
    protected override object Part1Impl()
    {
        var sum = 0;

        Dictionary<string, HashSet<string>> dict = new();
        var dictAlt = dict.GetAlternateLookup<ReadOnlySpan<char>>();
        var text = Input.Text;

        foreach (var splitRange in text.AsSpan().Split('\n'))
        {
            var i = splitRange.Start.Value;
            var key = text[i..(i + 2)];
            var value = text[(i + 3)..(i + 5)];
            
            ref var list = ref CollectionsMarshal.GetValueRefOrAddDefault(dictAlt, key, out _);
            list ??= new();
            list.Add(value);
            
            ref var list2 = ref CollectionsMarshal.GetValueRefOrAddDefault(dictAlt, value, out _);
            list2 ??= new();
            list2.Add(key);
        }
        
        foreach (var (first, conns) in dict)
        {
            bool firstHasT = first.StartsWith('t');
            foreach (var (second, conns2) in dict)
            {
                if (first == second)
                    break;
                bool secondHasT = firstHasT || second.StartsWith('t');
                foreach (var (third, conns3) in dict)
                {
                    if (first == third)
                        break;
                    if (second == third)
                        break;
                    bool thirdHasT = secondHasT || third.StartsWith('t');

                    if (thirdHasT)
                    {
                        if ((conns.Contains(second) && conns.Contains(third))
                            && (conns2.Contains(first) && conns2.Contains(third))
                            && (conns3.Contains(first) && conns3.Contains(second)))
                        {
                            sum++;
                        }
                    }
                }
            }
        }

        return sum; // 1337
    }

    protected override object Part2Impl()
    {
        Dictionary<string, HashSet<string>> dict = new();
        var dictAlt = dict.GetAlternateLookup<ReadOnlySpan<char>>();
        var text = Input.Text;

        foreach (var splitRange in text.AsSpan().Split('\n'))
        {
            var i = splitRange.Start.Value;
            var key = text[i..(i + 2)];
            var value = text[(i + 3)..(i + 5)];
            
            ref var list = ref CollectionsMarshal.GetValueRefOrAddDefault(dictAlt, key, out _);
            list ??= new();
            list.Add(value);
            
            ref var list2 = ref CollectionsMarshal.GetValueRefOrAddDefault(dictAlt, value, out _);
            list2 ??= new();
            list2.Add(key);
        }

        HashSet<string> curr = [];

        HashSet<string> largestSize = [];

        HashSet<string> memo = [];

        foreach (var (first, firstConn) in dict)
        {
            TryExpandParty(dict, curr, first, ref largestSize, memo);
        }

        return PartyCode(largestSize);
    }

    private static string PartyCode(HashSet<string> largestSize)
    {
        return string.Join(',', largestSize.Order());
    }

    bool TryExpandParty(Dictionary<string, HashSet<string>> dict, HashSet<string> party, string first, ref HashSet<string> largestSize,
        HashSet<string> memo)
    {
        if (party.Contains(first))
            return false;
        
        var canAdd = true;

        foreach (var existing in party)
        {
            var existingConn = dict[existing];
            if (existingConn.Contains(first))
                continue;
            
            canAdd = false;
            break;
        }

        if (!canAdd)
            return false;
        
        HashSet<string> newParty = [..party, first];
        var code = PartyCode(newParty);
        if (memo.Contains(code))
            return false;
        
        if (newParty.Count > largestSize.Count)
        {
            largestSize = newParty;
        }
        
        var anySuccess = false;
        foreach (var connected in dict[first])
        {
            if (TryExpandParty(dict, newParty, connected, ref largestSize, memo))
            {
                anySuccess = true;
            }
        }
        
        memo.Add(code);
        return anySuccess;
    }
}