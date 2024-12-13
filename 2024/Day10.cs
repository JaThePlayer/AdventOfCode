using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using CommunityToolkit.HighPerformance;

namespace AoC._2024;

/*
Initial
| Method | Mean     | Error    | StdDev   | Allocated |
|------- |---------:|---------:|---------:|----------:|
| Part1  | 54.77 us | 1.082 us | 2.282 us |      24 B |
| Part2  | 35.80 us | 0.350 us | 0.328 us |      24 B |

Part1: use id's to avoid clearing the `visited` grid.
| Method | Mean     | Error    | StdDev   | Allocated |
|------- |---------:|---------:|---------:|----------:|
| Part1  | 31.77 us | 0.556 us | 0.595 us |      24 B |
| Part2  | 35.41 us | 0.265 us | 0.248 us |      24 B |

Part2: memoize
| Method | Mean     | Error    | StdDev   | Allocated |
|------- |---------:|---------:|---------:|----------:|
| Part1  | 30.68 us | 0.066 us | 0.052 us |      24 B |
| Part2  | 18.95 us | 0.263 us | 0.246 us |      24 B |

STUPID generics
| Method | Mean     | Error    | StdDev   | Allocated |
|------- |---------:|---------:|---------:|----------:|
| Part1  | 24.51 us | 0.229 us | 0.214 us |      24 B |
| Part2  | 15.61 us | 0.083 us | 0.073 us |      24 B |

Part2 - use 'byte' for cache
| Part2  | 15.32 us | 0.022 us | 0.018 us |      24 B |

Small microopts
| Method | Mean     | Error    | StdDev   | Allocated |
|------- |---------:|---------:|---------:|----------:|
| Part1  | 23.89 us | 0.063 us | 0.056 us |      24 B |
| Part2  | 15.19 us | 0.069 us | 0.061 us |      24 B |
 */
public class Day10 : AdventBase
{
    public override int Year => 2024;
    public override int Day => 10;

    private interface ITarget
    {
        public static abstract char Value { get; }
    }
    
    private struct Target1 : ITarget
    {
        public static char Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => '1';
        }
    }
    
    private struct NextTarget<TInner> : ITarget
        where TInner : struct, ITarget
    {
        public static char Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (char)(TInner.Value + 1);
        }
    }
    
    private static int Count<TDirs, TTarget>(ReadOnlySpan2D<byte> span, Span2D<ushort> visited, ushort id, int sx, int sy)
        where TDirs : IDirPicker
        where TTarget : struct, ITarget
    {
        ref var visit = ref visited.DangerousGetReferenceAt(sy, sx);
        if (visit == id)
            return 0;
        visit = id;
        if (TTarget.Value == '9'+1)
            return 1;
        
        var r = 0;
        if (TDirs.Left && sx > 0 && span.DangerousGetReferenceAt(sy, sx-1) == TTarget.Value)
            r += Count<ExceptRightPicker, NextTarget<TTarget>>(span, visited, id, sx-1, sy);
        if (TDirs.Right && span.DangerousGetReferenceAt(sy, sx+1) == TTarget.Value)
            r += Count<ExceptLeftPicker, NextTarget<TTarget>>(span, visited, id, sx+1, sy);
        if (TDirs.Up && sy > 0 && span.DangerousGetReferenceAt(sy-1, sx) == TTarget.Value)
            r += Count<ExceptDownPicker, NextTarget<TTarget>>(span, visited, id, sx, sy-1);
        if (TDirs.Down && sy+1 < span.Height && span.DangerousGetReferenceAt(sy+1, sx) == TTarget.Value)
            r += Count<ExceptUpPicker, NextTarget<TTarget>>(span, visited, id, sx, sy+1);

        return r;
    }
    
    private static int Count2<TDirs, TTarget>(ReadOnlySpan2D<byte> span, Span2D<byte> visited, int sx, int sy)
        where TDirs : IDirPicker
        where TTarget : struct, ITarget
    {
        if (TTarget.Value == '9'+1)
            return 1;
        
        ref var cache = ref visited.DangerousGetReferenceAt(sy, sx);
        if (cache > 0)
            return cache - 1;

        var r = 0;
        if (TDirs.Left && sx > 0 && span.DangerousGetReferenceAt(sy, sx-1) == TTarget.Value)
            r += Count2<ExceptRightPicker, NextTarget<TTarget>>(span, visited, sx-1, sy);
        if (TDirs.Right && span.DangerousGetReferenceAt(sy, sx+1) == TTarget.Value)
            r += Count2<ExceptLeftPicker, NextTarget<TTarget>>(span, visited, sx+1, sy);
        if (TDirs.Up && sy > 0 && span.DangerousGetReferenceAt(sy-1, sx) == TTarget.Value)
            r += Count2<ExceptDownPicker, NextTarget<TTarget>>(span, visited, sx, sy-1);
        if (TDirs.Down && sy+1 < span.Height && span.DangerousGetReferenceAt(sy+1, sx) == TTarget.Value)
            r += Count2<ExceptUpPicker, NextTarget<TTarget>>(span, visited, sx, sy+1);

        cache = (byte)(r + 1);

        return r;
    }
    
    protected override object Part1Impl()
    {
        var sum = 0;
        var input = Input.TextU8;
        var lineWidth = input.IndexOf((byte)'\n') + 1;
        
        var span = ReadOnlySpan2D<byte>.DangerousCreate(input[0], input.Length / lineWidth + 1, lineWidth, 0);
        
        // we'll store the ID of the starting point in the array, so we don't have to clear it each time
        Span<ushort> visited1D = stackalloc ushort[span.Height * span.Width];
        var visited = Span2D<ushort>.DangerousCreate(ref visited1D[0], span.Height, span.Width, 0);

        ushort id = 1;
        int i;
        var si = 0;
        while ((i = input.IndexOf((byte)'0')) != -1)
        {
            si += i;
            var x = si % span.Width;
            var y = si / span.Width;
            sum += Count<AllDirPicker, Target1>(span, visited, id++, x, y);
            
            input = input[(i + 1)..];
            si++;
        }
        
        return sum; // 682
    }

    protected override object Part2Impl()
    {
        var sum = 0;
        var input = Input.TextU8;
        var lineWidth = input.IndexOf((byte)'\n') + 1;
        
        var span = ReadOnlySpan2D<byte>.DangerousCreate(input[0], input.Length / lineWidth + 1, lineWidth, 0);
        Span<byte> visited1D = stackalloc byte[span.Height * span.Width];
        var visited = Span2D<byte>.DangerousCreate(ref visited1D[0], span.Height, span.Width, 0);

        int i;
        var si = 0;
        while ((i = input.IndexOf((byte)'0')) != -1)
        {
            si += i;
            var x = si % span.Width;
            var y = si / span.Width;
            sum += Count2<AllDirPicker, Target1>(span, visited, x, y);
            
            input = input[(i + 1)..];
            si++;
        }
        
        return sum; // 1511
    }
}