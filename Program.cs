using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using AoC;
using AoC._2024;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Spectre.Console;

AdventBase[] days = [
    new Day01(),
    new Day02(),
    new Day03NoRegex(),
    new Day04(),
    new Day05(),
    new Day06(),
    new Day07Backwards(),
    new Day08(),
    new Day09(),
    new Day10(),
    new Day11(),
    new Day12(),
    new Day13(),
    new Day14(),
    new Day15(),
    new Day16(),
    new Day17(),
    new Day18(),
    new Day19(),
    new Day20(),
    new Day01(),
    new Day22(),
];

var day = days[22 - 1];
Console.WriteLine(day.Part1());
Console.WriteLine(day.Part2());
//return;
//for (int i = 0; i < 1_000_000; i++) day.Part2();

day.Benchmark();
return;

var t = new Spectre.Console.Table()
    .AddColumn("Day")
    .AddColumn("P1 (Cold)")
    .AddColumn("P2 (Cold)")
    .AddColumn("P1")
    .AddColumn("P2");

var dayInfos = new List<DayInfo>();

foreach (var d in days)
{
    t.AddRow(d.Day.ToString());
}

t.AddEmptyRow();
t.AddRow("Total");


var display = Spectre.Console.AnsiConsole.Live(t);
display.Start(ctx =>
{
    ctx.Refresh();
    foreach (var d in days)
    //Parallel.ForEach(days, d =>
    {
        _ = d.Input.Text;
        var curr = Stopwatch.GetTimestamp();
        d.Part1();
        var p1Cold = Stopwatch.GetElapsedTime(curr);
        t.UpdateCell(d.Day - 1, 1, FormatTime(p1Cold, false));
        ctx.Refresh();
        
        curr = Stopwatch.GetTimestamp();
        d.Part2();
        var p2Cold = Stopwatch.GetElapsedTime(curr);
        t.UpdateCell(d.Day - 1, 2, FormatTime(p2Cold, false));
        ctx.Refresh();

        int iterations = 0;
        var warmupStart = Stopwatch.GetTimestamp();
        while (Stopwatch.GetElapsedTime(warmupStart).TotalSeconds < 2)
        {
            d.Part1();
            d.Part2();
            iterations++;
        }

        curr = Stopwatch.GetTimestamp();
        for (int i = 0; i < iterations; i++)
            d.Part1();
        var p1 = Stopwatch.GetElapsedTime(curr) / iterations;
        t.UpdateCell(d.Day - 1, 3, FormatTime(p1, true));
        ctx.Refresh();
        
        curr = Stopwatch.GetTimestamp();
        for (int i = 0; i < iterations; i++)
            d.Part2();
        var p2 = Stopwatch.GetElapsedTime(curr) / iterations;
        t.UpdateCell(d.Day - 1, 4, FormatTime(p2, true));
        ctx.Refresh();
        
        dayInfos.Add(new DayInfo
        {
            Day = d,
            Part1 = p1,
            Part2 = p2,
            Part1Cold = p1Cold,
            Part2Cold = p2Cold,
        });


    }
    //);

    // Add total row
    var totalRowIdx = dayInfos.Count + 1;
    t.UpdateCell(totalRowIdx, 1, 
        FormatTimeMs(dayInfos.Select(d => d.Part1Cold).Aggregate(TimeSpan.Zero, (a,b)=>a + b), false));
    t.UpdateCell(totalRowIdx, 2, 
        FormatTimeMs(dayInfos.Select(d => d.Part2Cold).Aggregate(TimeSpan.Zero, (a,b)=>a + b), false));
    t.UpdateCell(totalRowIdx, 3, 
        FormatTimeMs(dayInfos.Select(d => d.Part1).Aggregate(TimeSpan.Zero, (a,b)=>a + b), false));
    t.UpdateCell(totalRowIdx, 4, 
        FormatTimeMs(dayInfos.Select(d => d.Part2).Aggregate(TimeSpan.Zero, (a,b)=>a + b), false));
    
    static string FormatTime(TimeSpan t, bool colored) =>
        @$"{(colored ? GetTimeColor(t) : "[white]")}{t.TotalMicroseconds.ToString(CultureInfo.InvariantCulture)}us[/]";
    
    static string FormatTimeMs(TimeSpan t, bool colored) =>
        @$"{(colored ? GetTimeColor(t) : "[white]")}{t.TotalMilliseconds.ToString(CultureInfo.InvariantCulture)}ms[/]";

    static string GetTimeColor(TimeSpan t) => t.TotalMicroseconds switch
    {
        < 50 => @"[green]",
        < 100 => @"[white]",
        _ => @"[red]",
    };
});

struct DayInfo
{
    public AdventBase Day;
    
    public TimeSpan Part1;
    public TimeSpan Part2;

    public TimeSpan Part1Cold;
    public TimeSpan Part2Cold;
}
