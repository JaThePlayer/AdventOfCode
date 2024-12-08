using System.Numerics;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace AoC;

public static class Util
{
    /// <summary>
    /// Int parsing function which assumes the input is correct
    /// </summary>
    public static T FastParseInt<T>(ReadOnlySpan<char> input) where T : IBinaryInteger<T>
    {
        var result = T.Zero;
        var ten = T.CreateTruncating(10);

        foreach (var c in input)
        {
            result = result * ten + T.CreateTruncating(c - '0');
        }
        
        return result;
    }
    
    public static T FastParse2DigitInt<T>(ReadOnlySpan<char> input) where T : IBinaryInteger<T>
    {
        return T.CreateTruncating(10) * T.CreateTruncating(input[0] - '0') + T.CreateTruncating(input[1] - '0');
    }
    
    /// <summary>
    /// Parse a list of integers into the given buffer.
    /// Assumes input is correct
    /// </summary>
    public static Span<T> FastParseIntList<T>(ReadOnlySpan<char> input, char separator, Span<T> buffer) where T : IBinaryInteger<T>
    {
        var i = 0;
        var value = T.Zero;
            
        foreach (var c in input)
        {
            if (c == separator)
            {
                buffer[i++] = value;
                value = T.Zero;
            }
            else
            {
                value = value * T.CreateTruncating(10) + T.CreateTruncating(c - '0');
            }
        }
        buffer[i++] = value;

        return buffer[..i];
    }
    
    /// <summary>
    /// Parse a list of integers into the given buffer.
    /// Assumes input is correct: 01,2 3,45
    /// </summary>
    public static unsafe Span<T> FastParse2DigitIntList<T>(ReadOnlySpan<char> input, Span<T> buffer) where T : unmanaged, IBinaryInteger<T>
    {
        var i = 0;
        var j = 0; 
        /*
        fixed (T* b = &buffer[0])
        {
            for (; j + Vector128<short>.Count < input.Length && (uint)(i + 3) < buffer.Length; j += Vector128<short>.Count + 1)
            {
                var t = Vector128.LoadUnsafe(in Unsafe.As<char, short>(ref Unsafe.AsRef(in input[j]))) - Vector128.Create<short>((short)'0');
                t = Vector128.Shuffle(t, Vector128.Create(0, 3, 6, 4, 1, 4, 7, 5));
                var (l, h) = Vector128.Widen(t);
                var ret = 10 * l + h;
                b[i++] = T.CreateTruncating(ret[0]);
                b[i++] = T.CreateTruncating(ret[1]);
                b[i++] = T.CreateTruncating(ret[2]);
            }


            for (; j+1 < input.Length; j += 3)
            {
                b[i++] = T.CreateTruncating(10) * T.CreateTruncating(input[j] - '0') + T.CreateTruncating(input[j+1] - '0');
            }
        }
        */
        for (; j+1 < input.Length; j += 3)
        {
            buffer[i++] = T.CreateTruncating(10) * T.CreateTruncating(input[j] - '0') + T.CreateTruncating(input[j+1] - '0');
        }
        return buffer[..i];
    }

    public static void LogAsJson<T>(this T a)
    {
        Console.WriteLine(JsonSerializer.Serialize(a, _opts));
    }

    private static readonly JsonSerializerOptions _opts = new()
    {
        IncludeFields = true,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
}