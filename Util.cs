using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace AoC;

public static class Util
{
    /// <summary>
    /// Int parsing function which assumes the input is correct
    /// </summary>
    public static T FastParseInt<T>(ReadOnlySpan<char> input) where T : IBinaryInteger<T>
        => FastParseInt<T, char>(input);
    
    /// <inheritdoc cref="FastParseInt{T}(System.ReadOnlySpan{char})" />
    public static T FastParseInt<T>(ReadOnlySpan<byte> input) where T : IBinaryInteger<T>
        => FastParseInt<T, byte>(input);


    /// <inheritdoc cref="FastParseInt{T}(System.ReadOnlySpan{char})" />
    public static T FastParseInt<T, TChar>(ReadOnlySpan<TChar> input) 
        where T : IBinaryInteger<T>
        where TChar : IBinaryInteger<TChar>
    {
        var result = T.Zero;
        var ten = T.CreateTruncating(10);
        var zero = TChar.CreateTruncating('0');

        foreach (var c in input)
        {
            result = result * ten + T.CreateTruncating(c - zero );
        }
        
        return result;
    }
    
    
    public static T FastParse2DigitInt<T>(ReadOnlySpan<char> input) where T : IBinaryInteger<T>
    {
        return T.CreateTruncating(10) * T.CreateTruncating(input[0] - '0') + T.CreateTruncating(input[1] - '0');
    }
    
    public static T FastParse2DigitInt<T>(ReadOnlySpan<byte> input) where T : IBinaryInteger<T>
    {
        return T.CreateTruncating(10) * T.CreateTruncating(input[0] - '0') + T.CreateTruncating(input[1] - '0');
    }
    
    /// <summary>
    /// Parse a list of integers into the given buffer.
    /// Assumes input is correct
    /// </summary>
    public static Span<T> FastParseIntList<T, TChar>(ReadOnlySpan<TChar> input, TChar separator, Span<T> buffer) 
        where T : IBinaryInteger<T>
        where TChar : IBinaryInteger<TChar>
    {
        var i = 0;
        var value = T.Zero;
        var zero = TChar.CreateTruncating('0');
            
        foreach (var c in input)
        {
            if (c == separator)
            {
                buffer[i++] = value;
                value = T.Zero;
            }
            else
            {
                value = value * T.CreateTruncating(10) + T.CreateTruncating(c - zero);
            }
        }
        buffer[i++] = value;

        return buffer[..i];
    }
    
    /// <summary>
    /// Parse a list of integers into the given buffer.
    /// Assumes input is correct: 01,2 3,45
    /// </summary>
    public static Span<T> FastParse2DigitIntList<T, TChar>(ReadOnlySpan<TChar> input, Span<T> buffer)
        where T : unmanaged, IBinaryInteger<T>
        where TChar : unmanaged, IBinaryInteger<TChar>
    {
        var zero = TChar.CreateTruncating('0');
        var i = 0;
        var j = 0;
        for (; j+1 < input.Length; j += 3)
        {
            buffer[i++] = T.CreateTruncating(10) * T.CreateTruncating(input[j] - zero) + T.CreateTruncating(input[j+1] - zero);
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