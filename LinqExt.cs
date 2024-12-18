using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace AoC;

public static class LinqExt
{
    public static Dictionary<TKey, int> ToCountByDictionary<TSource, TKey>(this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector, int? initialCapacity = null) where TKey : notnull
    {
        var dict = initialCapacity is {} i ? new Dictionary<TKey, int>(i) : new Dictionary<TKey, int>();

        switch (source)
        {
            case List<TSource> list:
            {
                foreach (var obj in CollectionsMarshal.AsSpan(list))
                    CollectionsMarshal.GetValueRefOrAddDefault(dict, keySelector(obj), out _)++;
                return dict;
            }
            case TSource[] arr:
            {
                foreach (var obj in arr.AsSpan())
                    CollectionsMarshal.GetValueRefOrAddDefault(dict, keySelector(obj), out _)++;
                return dict;
            }
        }

        foreach (var obj in source)
        {
            CollectionsMarshal.GetValueRefOrAddDefault(dict, keySelector(obj), out _)++;
        }
        
        return dict;
    }
    
    /// <summary>
    /// Enumerates over all splits in <paramref name="input"/> using the <paramref name="separator"/>,
    /// and yields the result of calling the <paramref name="parser"/> on each split.
    /// </summary>
    public static SplitParser<TInput, TState, TResult> ParseSplits<TInput, TState, TResult>(this ReadOnlySpan<TInput> input, TInput separator, TState state,
        Func<ReadOnlySpan<TInput>, TState, TResult> parser) 
        where TState : allows ref struct
        where TResult : allows ref struct
        where TInput : IEquatable<TInput>
    {
        return new SplitParser<TInput, TState, TResult>(input, separator, state, parser);
    }


    public static SlimSplitEnumerator<T> SplitSlim<T>(this ReadOnlySpan<T> arr, T separator) where T : IEquatable<T>
    {
        return new(arr, separator);
    }

    public static bool SplitTwo<T>(this ReadOnlySpan<T> input, T sep, out ReadOnlySpan<T> left, out ReadOnlySpan<T> right) 
        where T : IEquatable<T>
    {
        var idx = input.IndexOf(sep);
        if (idx == -1) {
            left = input;
            right = ReadOnlySpan<T>.Empty;
            return false;
        }

        left = input[..idx];
        right = input[(idx + 1)..];
        return true;
    }

    public static bool ParseTwoSplits<T, TRet>(this ReadOnlySpan<T> input, T sep, Func<ReadOnlySpan<T>, TRet> parse,
        [NotNullWhen(true)] out TRet? left, [NotNullWhen(true)] out TRet? right)
        where T : IEquatable<T>
    {
        var idx = input.IndexOf(sep);
        if (idx == -1) {
            left = default!;
            right = default!;
            return false;
        }

        left = parse(input[..idx])!;
        right = parse(input[(idx + 1)..])!;
        return true;
    }
    
    
    public ref struct SplitParser<TInput, TState, TResult>(
        ReadOnlySpan<TInput> input,
        TInput separator,
        TState state,
        Func<ReadOnlySpan<TInput>, TState, TResult> parser) 
        where TState : allows ref struct
        where TResult : allows ref struct
        where TInput : IEquatable<TInput>
    {
        private MemoryExtensions.SpanSplitEnumerator<TInput> _enumerator = input.Split(separator);
        private ReadOnlySpan<TInput> _input = input;
        private TState _state = state;
        private TResult _current;

        public TResult Current => _current;
        
        public SplitParser<TInput, TState, TResult> GetEnumerator()
        {
            return new(_input, separator, _state, parser);
        }

        public bool MoveNext()
        {
            ref var enumerator = ref _enumerator;
            if (enumerator.MoveNext())
            {
                var line = _input[enumerator.Current];
                _current = parser(line, _state);
                return true;
            }

            _current = default!;
            return false;
        }
    }
    
    public ref struct SlimSplitEnumerator<T>(ReadOnlySpan<T> input, T separator) : ISpanEnumerator<ReadOnlySpan<T>, SlimSplitEnumerator<T>>
        where T : IEquatable<T>
    {
        private ReadOnlySpan<T> _input = input;
        private int _currentStart;
        private int _currentEnd = -1;

        public bool MoveNext(out ReadOnlySpan<T> res)
        {
            if (_currentEnd == _input.Length)
            {
                res = default;
                return false;
            }
            var startIdx = _currentEnd + 1;
            var nextIdx = _input[startIdx..].IndexOf(separator);
            _currentStart = _currentEnd + 1;
            _currentEnd = nextIdx >= 0 ? startIdx + nextIdx : _input.Length;
            res = _input[_currentStart.._currentEnd];
            return true;
        }

        public SpanEnumerable<ReadOnlySpan<T>, SlimSplitEnumerator<T>> AsEnumerable()
        {
            return new(this);
        }

        public SelectSEnumerator<TNext, ReadOnlySpan<T>, SlimSplitEnumerator<T>> Select<TNext>(Func<ReadOnlySpan<T>, TNext> selector)
            where TNext : allows ref struct
        {
            return new(this, selector);
        }
    }
}

public interface ISpanEnumerator<T, TSelf> 
    where T : allows ref struct 
    where TSelf : ISpanEnumerator<T, TSelf>, allows ref struct
{
    public bool MoveNext(out T res);
    
    public SpanEnumerable<T, TSelf> AsEnumerable()
    {
        if (this is TSelf spanEnumerator)
            return new(spanEnumerator);
        throw new Exception("Yo what");
    }
    
    public SelectSEnumerator<TNext, T, TSelf> Select<TNext>(Func<T, TNext> selector)
        where TNext : allows ref struct
    {
        if (this is TSelf spanEnumerator)
            return new(spanEnumerator, selector);
        throw new Exception("Yo what");
    }
}

public static class SpanEnumeratorExt
{
    public static Span<T> FillWith<T, TEnum>(this Span<T> buffer, SpanEnumerable<T, TEnum> e)
        where TEnum : ISpanEnumerator<T, TEnum>, allows ref struct
    {
        var i = 0;
        if (buffer.IsEmpty)
            return buffer;
        
        foreach (var t in e)
        {
            buffer[i++] = t;
            if (i == buffer.Length)
            {
                return buffer;
            }
        }
        
        return buffer[..i];
    }
    
    public static Span<T> FillSpan<T, TEnum>(this TEnum e, Span<T> buffer)
        where TEnum : ISpanEnumerator<T, TEnum>, allows ref struct
    {
        return buffer.FillWith(e.AsEnumerable());
    }
    
    public static List<T> ToList<T, TEnum>(this TEnum span, T? t = default) where TEnum : ISpanEnumerator<T, TEnum>, allows ref struct
    {
        var ret = new List<T>();
        foreach (var v in span.AsEnumerable())
        {
            ret.Add(v);
        }

        return ret;
    }
}

public ref struct SpanEnumerable<T, TEnum>(TEnum enumerator)
    where TEnum : ISpanEnumerator<T, TEnum>, allows ref struct
    where T : allows ref struct
{
    private TEnum _enumerator = enumerator;
    private T _current;
    
    public T Current => _current;
        
    public SpanEnumerable<T, TEnum> GetEnumerator()
    {
        return this;
    }

    public bool MoveNext()
    {
        return _enumerator.MoveNext(out _current);
    }
}

public ref struct SelectSEnumerator<T, TInnerKey, TInner>(TInner input, Func<TInnerKey, T> selector) 
    : ISpanEnumerator<T, SelectSEnumerator<T, TInnerKey, TInner>>
    where TInner : ISpanEnumerator<TInnerKey, TInner>, allows ref struct
    where T : allows ref struct
    where TInnerKey : allows ref struct
{
    private TInner _input = input;

    public bool MoveNext(out T res)
    {
        if (_input.MoveNext(out var key))
        {
            res = selector(key);
            return true;
        }

        res = default!;
        return false;
    }

    public SpanEnumerable<T, SelectSEnumerator<T, TInnerKey, TInner>> AsEnumerable()
    {
        return new(this);
    }

    public SelectSEnumerator<TNext, T, SelectSEnumerator<T, TInnerKey, TInner>> Select<TNext>(Func<T, TNext> selector)
        where TNext : allows ref struct
    {
        return new(this, selector);
    }
}