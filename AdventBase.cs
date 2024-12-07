using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace AoC;

[MemoryDiagnoser]
[SimpleJob]
public abstract class AdventBase
{
    public abstract int Year { get; }
    public abstract int Day { get; }

    private AdventFile? _input, _exampleInput;

    public AdventFile Input => _input ??= new($"{ProjectRoot}/{Year}/Inputs/{(Day >= 10 ? Day.ToString() : $"0{Day}")}.txt");
    public AdventFile ExampleInput => _exampleInput ??= new($"{ProjectRoot}/{Year}/Inputs/{(Day < 10 ? Day.ToString() : $"0{Day}")}.example.txt");

   // [Benchmark]
    public object Part1() => Part1Impl();
    
    protected abstract object Part1Impl();
    
    [Benchmark]
    public object Part2() => Part2Impl();
    
    protected abstract object Part2Impl();

    public void Benchmark()
    {
        BenchmarkRunner.Run(GetType());
    }

    private static string? _projectRoot;
    
    internal static string ProjectRoot
    {
        get
        {
            if (_projectRoot != null)
                return _projectRoot;
            for (var directoryInfo = new DirectoryInfo(Directory.GetCurrentDirectory()); directoryInfo != null; directoryInfo = directoryInfo.Parent)
            {
                var files = directoryInfo.GetFiles("*.csproj", SearchOption.TopDirectoryOnly);
                if (files.Length != 0)
                {
                    if (files[0].Name.StartsWith("BenchmarkDotNet"))
                    {
                        Console.SetOut(TextWriter.Null);
                    }
                    else
                    {
                        _projectRoot = Path.GetDirectoryName(files[0].FullName);
                        break;
                    }
                }
            }
            return _projectRoot ?? throw new DirectoryNotFoundException("Can't find project root.");
        }
    }
}

public sealed class AdventFile(string path)
{
    private string? _text;
    
    public string Text => _text ??= File.ReadAllText(path).ReplaceLineEndings("\n");
}