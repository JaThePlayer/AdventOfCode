using System.Globalization;
using System.Runtime.CompilerServices;

namespace AoC;

using ParserRes = Res<SpanParser>;
using ParserResU8 = Res<SpanParserU8>;

internal ref struct SpanParser(ReadOnlySpan<char> input)
{
    private ReadOnlySpan<char> _remaining = input;
    
    public ReadOnlySpan<char> Remaining => _remaining;
    
    public bool IsEmpty => _remaining.IsEmpty;

    private Res<T> ReadSlice<T>(IFormatProvider? format, int len) where T : ISpanParsable<T>
    {
        var success = T.TryParse(_remaining[..len], format ?? CultureInfo.InvariantCulture, out var ret);

        //Remaining = Remaining.Length >= len ? Remaining[len..] : Remaining[(len + 1)..];
        _remaining = _remaining[len..];
        
        if (!success)
        {
            return new(default!, false);
        }

        return new Res<T>(ret!, true);
    }
    
    private ReadOnlySpan<char> ReadSliceStr(int len)
    {
        var ret = _remaining[..len];
        _remaining = _remaining[len..];

        return ret;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Res<T> Read<T>(IFormatProvider? format = null) where T : ISpanParsable<T>
        => ReadSlice<T>(format, _remaining.Length);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryRead<T>(out T parsed, IFormatProvider? format = null) where T : ISpanParsable<T>
        => Read<T>(format).TryUnpack(out parsed);
    
    /// <summary>
    /// Reads the remaining span to completion, returning that span.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<char> ReadStr()
        => ReadSliceStr(_remaining.Length);

    public Res<T> ReadUntil<T>(char until, IFormatProvider? format = null) where T : ISpanParsable<T>
    {
        var rem = _remaining;
        var len = rem.IndexOf(until);
        if (len == -1)
            return ReadSlice<T>(format, rem.Length);
        
        var ret = ReadSlice<T>(format, len);
        _remaining = _remaining[1..];

        return ret;
    }
    
    public ReadOnlySpan<char> ReadStrUntil(char until)
    {
        var rem = _remaining;
        var len = rem.IndexOf(until);
        if (len == -1)
            return ReadSliceStr(rem.Length);
        
        var ret = ReadSliceStr(len);
        _remaining = _remaining[1..];

        return ret;
    }
    
    public ReadOnlySpan<char> ReadStrUntilAny(ReadOnlySpan<char> until)
    {
        var rem = _remaining;
        var len = rem.IndexOfAny(until);
        if (len == -1)
            return ReadSliceStr(rem.Length);
        
        var ret = ReadSliceStr(len);
        _remaining = _remaining[1..];

        return ret;
    }

    /// <summary>
    /// Returns a new parser, which contains a slice of the span from the current location to the location of the next <paramref name="until"/> character.
    /// </summary>
    public ParserRes SliceUntil(char until)
    {
        var rem = _remaining;
        if (rem.Length == 0)
            return ParserRes.Error();
        
        var len = rem.IndexOf(until);
        if (len < 0)
        {
            // read until the end
            _remaining = ReadOnlySpan<char>.Empty;
            return ParserRes.Ok(new(rem));
        }
        
        _remaining = rem[(len+1)..]; // +1 to skip past the 'until' character
        return ParserRes.Ok(new(rem[..len]));
    }
    
    /// <summary>
    /// Returns a new parser, which contains a slice of the span from the current location to the location of the next <paramref name="until"/> character.
    /// </summary>
    public ParserRes SliceUntilAny(ReadOnlySpan<char> until, out char splitChar) {
        splitChar = '\0';
        var rem = _remaining;
        if (rem.Length == 0)
            return ParserRes.Error();
        
        var len = rem.IndexOfAny(until);
        if (len < 0)
        {
            // read until the end
            _remaining = ReadOnlySpan<char>.Empty;
            return ParserRes.Ok(new(rem));
        }

        splitChar = rem[len];
        _remaining = rem[(len+1)..]; // +1 to skip past the 'until' character
        return ParserRes.Ok(new(rem[..len]));
    }
    
    public ParserRes SliceUntilAnyOutsideBrackets(ReadOnlySpan<char> until, out char splitChar, int skipFirst = 0) {
        splitChar = '\0';
        var rem = _remaining[skipFirst..];
        if (rem.Length == 0)
            return ParserRes.Error();

        var untilWithBrackets = $"{until}()";
        var bracketDepth = 0;

        var skipped = 0;
        
        while (true)
        {
            var len = rem.IndexOfAny(untilWithBrackets);
            if (len < 0)
            {
                if (bracketDepth == 0) {
                    // read until the end
                    var result = _remaining;
                    _remaining = ReadOnlySpan<char>.Empty;
                    return ParserRes.Ok(new(result));
                }
                
                return ParserRes.Error();
            }

            if (rem[len] == '(')
            {
                bracketDepth++;
            } else if (rem[len] == ')')
            {
                bracketDepth--;
            }
            else
            {
                if (bracketDepth <= 0)
                {
                    splitChar = rem[len];

                    var result = _remaining[..(len+skipped)];
        
                    _remaining = rem[(len+1)..]; // +1 to skip past the 'until' character
                    return ParserRes.Ok(new(result));
                }
            }

            skipped += len + 1;
            rem = rem[(len + 1)..];
        }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool StartsWith(ReadOnlySpan<char> prefix)
        => _remaining.StartsWith(prefix);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool EndsWith(ReadOnlySpan<char> prefix)
        => _remaining.EndsWith(prefix);

    public void TrimStart()
    {
        _remaining = _remaining.TrimStart();
    }
    
    public void TrimEnd()
    {
        _remaining = _remaining.TrimEnd();
    }

    public void Skip(int chars) {
        _remaining = _remaining[chars..];
    }
    
    public void SkipEnd(int chars) {
        _remaining = _remaining[..^chars];
    }
}

internal readonly ref struct Res<T>(T val, bool success) where T : allows ref struct
{
    private readonly T _value = val;
    
    
    public bool IsSuccess => success;
    
    private T ValOrDef => _value;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Or(T def) => success ? _value : def;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryUnpack(out T ret)
    {
        ret = ValOrDef;
        return success;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator T(Res<T> res) => res.IsSuccess ? res.ValOrDef : throw new SpanParseFailException();
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Res<T> Error() => default;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Res<T> Ok(T res) => new(res, true);
}


internal class SpanParseFailException : Exception
{
    public override string Message => "Tried to get value from Res<T> when its IsSuccess property was false.";
}

internal ref struct SpanParserU8(ReadOnlySpan<byte> input)
{
    private ReadOnlySpan<byte> _remaining = input;
    
    public ReadOnlySpan<byte> Remaining => _remaining;
    
    public bool IsEmpty => _remaining.IsEmpty;

    private Res<T> ReadSlice<T>(IFormatProvider? format, int len) where T : IUtf8SpanParsable<T>
    {
        var success = T.TryParse(_remaining[..len], format ?? CultureInfo.InvariantCulture, out var ret);

        //Remaining = Remaining.Length >= len ? Remaining[len..] : Remaining[(len + 1)..];
        _remaining = _remaining[len..];
        
        if (!success)
        {
            return new(default!, false);
        }

        return new Res<T>(ret!, true);
    }
    
    private ReadOnlySpan<byte> ReadSliceStr(int len)
    {
        var ret = _remaining[..len];
        _remaining = _remaining[len..];

        return ret;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Res<T> Read<T>(IFormatProvider? format = null) where T : IUtf8SpanParsable<T>
        => ReadSlice<T>(format, _remaining.Length);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryRead<T>(out T parsed, IFormatProvider? format = null) where T : IUtf8SpanParsable<T>
        => Read<T>(format).TryUnpack(out parsed);
    
    /// <summary>
    /// Reads the remaining span to completion, returning that span.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<byte> ReadStr()
        => ReadSliceStr(_remaining.Length);

    public Res<T> ReadUntil<T>(byte until, IFormatProvider? format = null) where T : IUtf8SpanParsable<T>
    {
        var rem = _remaining;
        var len = rem.IndexOf(until);
        if (len == -1)
            return ReadSlice<T>(format, rem.Length);
        
        var ret = ReadSlice<T>(format, len);
        _remaining = _remaining[1..];

        return ret;
    }
    
    public ReadOnlySpan<byte> ReadStrUntil(char until)
    {
        var rem = _remaining;
        var len = rem.IndexOf((byte)until);
        if (len == -1)
            return ReadSliceStr(rem.Length);
        
        var ret = ReadSliceStr(len);
        _remaining = _remaining[1..];

        return ret;
    }
    
    public ReadOnlySpan<byte> ReadStrUntilAny(ReadOnlySpan<byte> until)
    {
        var rem = _remaining;
        var len = rem.IndexOfAny(until);
        if (len == -1)
            return ReadSliceStr(rem.Length);
        
        var ret = ReadSliceStr(len);
        _remaining = _remaining[1..];

        return ret;
    }

    /// <summary>
    /// Returns a new parser, which contains a slice of the span from the current location to the location of the next <paramref name="until"/> character.
    /// </summary>
    public ParserResU8 SliceUntil(byte until)
    {
        var rem = _remaining;
        if (rem.Length == 0)
            return ParserResU8.Error();
        
        var len = rem.IndexOf(until);
        if (len < 0)
        {
            // read until the end
            _remaining = ReadOnlySpan<byte>.Empty;
            return ParserResU8.Ok(new(rem));
        }
        
        _remaining = rem[(len+1)..]; // +1 to skip past the 'until' character
        return ParserResU8.Ok(new(rem[..len]));
    }
    
    /// <summary>
    /// Returns a new parser, which contains a slice of the span from the current location to the location of the next <paramref name="until"/> character.
    /// </summary>
    public ParserResU8 SliceUntilAny(ReadOnlySpan<byte> until, out byte splitChar) {
        splitChar = 0;
        var rem = _remaining;
        if (rem.Length == 0)
            return ParserResU8.Error();
        
        var len = rem.IndexOfAny(until);
        if (len < 0)
        {
            // read until the end
            _remaining = [];
            return ParserResU8.Ok(new(rem));
        }

        splitChar = rem[len];
        _remaining = rem[(len+1)..]; // +1 to skip past the 'until' character
        return ParserResU8.Ok(new(rem[..len]));
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool StartsWith(ReadOnlySpan<byte> prefix)
        => _remaining.StartsWith(prefix);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool EndsWith(ReadOnlySpan<byte> prefix)
        => _remaining.EndsWith(prefix);

    public void TrimStart()
    {
        _remaining = _remaining.TrimStart(" \n\r\t"u8);
    }
    
    public void TrimEnd()
    {
        _remaining = _remaining.TrimEnd(" \n\r\t"u8);
    }

    public void Skip(int chars) {
        _remaining = _remaining[chars..];
    }
    
    public void SkipEnd(int chars) {
        _remaining = _remaining[..^chars];
    }
    
    public List<T> ParseList<T>(char separator) where T : IUtf8SpanParsable<T> {
        List<T> arr = [];
        while (!IsEmpty && ReadUntil<T>((byte)separator).TryUnpack(out var t)) {
            arr.Add(t);
        }
        return arr;
    }
}