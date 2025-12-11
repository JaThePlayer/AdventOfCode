namespace AoC._2025;

/*
Initial
| Method | Mean      | Error    | StdDev   | Gen0    | Gen1    | Allocated |
|------- |----------:|---------:|---------:|--------:|--------:|----------:|
| Part1  |  69.41 us | 1.387 us | 2.502 us | 26.0010 | 10.3760 | 212.71 KB |
| Part2  | 201.65 us | 3.801 us | 3.904 us | 41.5039 | 20.5078 |  340.9 KB |
 */
public class Day11 : AdventBase
{
    public override int Year => 2025;
    public override int Day => 11;
    
    protected override object Part1Impl()
    {
        Dictionary<string, string[]> machines = [];
        
        var input = Input.Text;
        foreach (var line in input.Split('\n'))
        {
            var p = new SpanParser(line);
            var key = p.ReadStrUntil(':').ToString();
            p.TrimStart();
            p.TrimEnd();
            
            machines.Add(key, p.Remaining.ToString().Split(' '));
        }

        return Visit("you"); // 699

        long Visit(string machine)
        {
            if (machine == "out")
                return 1;

            long amt = 0;
            foreach (var connected in machines[machine])
                amt += Visit(connected);

            return amt;
        }
    }

    protected override object Part2Impl()
    {
        Dictionary<string, string[]> machines = [];
        
        var input =Input.Text;
        foreach (var line in input.Split('\n'))
        {
            var p = new SpanParser(line);
            var key = p.ReadStrUntil(':').ToString();
            p.TrimStart();
            p.TrimEnd();
            
            machines.Add(key, p.Remaining.ToString().Split(' '));
        }

        Dictionary<(string, bool, bool), long> memo = [];

        return Visit("svr", false, false); // 388893655378800

        long Visit(string machine, bool foundFft, bool foundDac)
        {
            if (!foundDac && machine == "dac")
                foundDac = true;
            if (!foundFft && machine == "fft")
                foundFft = true;
            
            if (memo.TryGetValue((machine, foundFft, foundDac), out var cached))
                return cached;

            if (machine == "out")
            {
                var r = foundDac && foundFft ? 1 : 0;
                memo[(machine, foundFft, foundDac)] = r;
                return r;
            }

            long amt = 0;
            foreach (var connected in machines[machine])
                amt += Visit(connected, foundFft, foundDac);

            memo[(machine, foundFft, foundDac)] = amt;
            return amt;
        }
    }
}