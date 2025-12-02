namespace AoC._2025;

/*
 Initial:
| Method | Mean      | Error    | StdDev   | Gen0        | Allocated  |
|------- |----------:|---------:|---------:|------------:|-----------:|
| Part1  |  23.20 ms | 0.454 ms | 0.831 ms |  11406.2500 |   91.16 MB |
| Part2  | 431.68 ms | 8.528 ms | 9.821 ms | 188000.0000 | 1500.45 MB |

Utf8
| Method | Mean      | Error    | StdDev   | Gen0        | Allocated  |
|------- |----------:|---------:|---------:|------------:|-----------:|
| Part1  |  23.90 ms | 0.452 ms | 0.484 ms |  11406.2500 |   91.16 MB |
| Part2  | 484.95 ms | 9.301 ms | 7.767 ms | 188000.0000 | 1500.45 MB |
 */
public class Day02 : AdventBase
{
    public override int Year => 2025;
    
    public override int Day => 2;
    
    protected override object Part1Impl()
    {
        var input = Input.TextU8;

        long res = 0;
        
        foreach (var rangeRange in input.Split((byte)','))
        {
            var range = input[rangeRange];
            _ = range.ParseTwoSplits((byte)'-', Util.FastParseInt<long, byte>, out var firstId, out var lastId);

            for (var id = firstId; id <= lastId; id++)
            {
                var str = id.ToString().AsSpan();
                if (str.Length % 2 == 1)
                    continue;
                
                var left = str[..(str.Length/2)];
                var right = str[(str.Length/2)..];

                if (left.SequenceEqual(right))
                    res += id;

                /*
                var invalid = id switch
                {
                    // 1 digit
                    < 10 => false,
                    // 2 digit
                    < 100 => str[0] == str[1],
                    // 3 digit
                    < 1000 => false,
                    // 4 digit
                    < 10000 => str[0] == str[3] && str[1] == str[2],
                    _ => throw new ArgumentOutOfRangeException(nameof(id), id, null)
                };
                */
            }
        }

        return res; // 40055209690
    }

    protected override object Part2Impl()
    {
        var input = Input.TextU8;

        long res = 0;
        
        foreach (var rangeRange in input.Split((byte)','))
        {
            var range = input[rangeRange];
            _ = range.ParseTwoSplits((byte)'-', Util.FastParseInt<long, byte>, out var firstId, out var lastId);

            for (var id = firstId; id <= lastId; id++)
            {
                var str = id.ToString();
                
                for (var chunkLen = 1; chunkLen < str.Length / 2 + 1; chunkLen++)
                {
                    using var chunks = str.Chunk(chunkLen).GetEnumerator();
                    _ = chunks.MoveNext();
                    var first = chunks.Current;
                    var allChunksSame = true;
                    while (chunks.MoveNext())
                    {
                        if (!chunks.Current.SequenceEqual(first))
                        {
                            allChunksSame = false;
                            break;
                        }
                    }

                    if (allChunksSame)
                    {
                        res += id;
                        break;
                    }
                }
            }
        }

        return res; // 50857215650
    }
}