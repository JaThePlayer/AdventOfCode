using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;

namespace AoC;

public static class MathExt
{
    public static ulong PowerOfTen(int pow)
    {
        ReadOnlySpan<ulong> powersOf10 =
        [
            1,
            10,
            100,
            1000,
            10000,
            100000,
            1000000,
            10000000,
            100000000,
            1000000000,
            10000000000,
            100000000000,
            1000000000000,
            10000000000000,
            100000000000000,
            1000000000000000,
            10000000000000000,
            100000000000000000,
            1000000000000000000,
            10000000000000000000,
        ];
        
        return Unsafe.Add(ref MemoryMarshal.GetReference(powersOf10), pow);
    }
    
    /// <summary>
    /// Counts how many digits are in this long (in base 10)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CountDigits(this ulong value)
    {
        // Ported from https://github.com/tesselslate/aoc-codspeed/blob/main/src/day11.rs
        // Original algo from https://da-data.blogspot.com/2023/02/integer-log10-in-rust-and-c.html
        ReadOnlySpan<ulong> lut = [
            9,                  99,                 999,
            9_999,              99_999,             999_999,
            9_999_999,          99_999_999,         999_999_999,
            9_999_999_999,      99_999_999_999,     999_999_999_999,
            9_999_999_999_999,  99_999_999_999_999, 999_999_999_999_999,
        ];
        const long mask = 0b0001001001000100100100010010010001001001000100100100010010010000;

        var guess = (int)long.PopCount(mask << (int)ulong.LeadingZeroCount(value));
        return guess + (value > lut.DangerousGetReferenceAt(guess) ? 2 : 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong NextPowerOf10(this ulong value)
    {
        ReadOnlySpan<ulong> powersOf10 =
        [
            10, 10, 10, 10, 10,
            100, 100, 100,
            1000, 1000, 1000, 1000,
            10000, 10000, 10000,
            100000, 100000, 100000,
            1000000, 1000000, 1000000, 1000000,
            10000000, 10000000, 10000000,
            100000000, 100000000, 100000000,
            1000000000, 1000000000, 1000000000, 1000000000,
            10000000000, 10000000000, 10000000000,
            100000000000, 100000000000, 100000000000,
            1000000000000, 1000000000000, 1000000000000, 1000000000000,
            10000000000000, 10000000000000, 10000000000000,
            100000000000000, 100000000000000, 100000000000000,
            1000000000000000, 1000000000000000, 1000000000000000, 1000000000000000,
            10000000000000000, 10000000000000000, 10000000000000000,
            100000000000000000, 100000000000000000, 100000000000000000,
            1000000000000000000, 1000000000000000000, 1000000000000000000, 1000000000000000000,
            10000000000000000000,
        ];
        ref var powerOf10 = ref Unsafe.Add(ref MemoryMarshal.GetReference(powersOf10), BitOperations.Log2(value));
        return Unsafe.Add(ref powerOf10, Unsafe.BitCast<bool,byte>(value >= powerOf10)*3);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Concat(this ulong left, ulong right) => left * NextPowerOf10(right) + right;

    public static void Transpose<T>(this ReadOnlySpan2D<T> b, Span2D<T> output)
    {
        for (var i = 0; i < b.Height; ++i)
            b.GetRowSpan(i).CopyTo(output.GetColumn(i));
    }
}