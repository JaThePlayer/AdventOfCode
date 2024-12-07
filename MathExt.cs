using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AoC;

public static class MathExt
{
    // From System.Buffers.Text.FormattingHelpers.CountDigits
    /// <summary>
    /// Counts how many digits are in this long (in base 10)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CountDigits(this ulong value)
    {
        // Map the log2(value) to a power of 10.
        ReadOnlySpan<byte> log2ToPow10 =
        [
            1, 1, 1, 2, 2, 2, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5,
            6, 6, 6, 7, 7, 7, 7, 8, 8, 8, 9, 9, 9, 10, 10, 10,
            10, 11, 11, 11, 12, 12, 12, 13, 13, 13, 13, 14, 14, 14, 15, 15,
            15, 16, 16, 16, 16, 17, 17, 17, 18, 18, 18, 19, 19, 19, 19, 20
        ];

        uint index = Unsafe.Add(ref MemoryMarshal.GetReference(log2ToPow10), BitOperations.Log2(value));

        // Read the associated power of 10.
        ReadOnlySpan<ulong> powersOf10 =
        [
            0, // unused entry to avoid needing to subtract
            0,
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
        var powerOf10 = Unsafe.Add(ref MemoryMarshal.GetReference(powersOf10), index);

        // Return the number of digits based on the power of 10, shifted by 1
        // if it falls below the threshold.
        return (int)(index - Unsafe.BitCast<bool, byte>(value < powerOf10));
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
}