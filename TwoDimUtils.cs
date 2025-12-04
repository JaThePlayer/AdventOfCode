using System.Runtime.CompilerServices;
using CommunityToolkit.HighPerformance;

namespace AoC;

public interface ITileVisitor<T>
{
    public void Visit(ref T value, int y, int x);
}

public interface ITileFilter<T>
{
    public bool Matches(T tile);
}

public class TwoDimUtils
{
    public static void ForEachNeighbor<TChar, TVisitor>(ref Span2D<TChar> map, ref TVisitor visitor, int y, int x)
        where TVisitor : struct, ITileVisitor<TChar>, allows ref struct
    {
        ref var middle = ref map[y, x];
        if (y > 0)
        {
            ref var el = ref Unsafe.Subtract(ref middle, map.Width);
            visitor.Visit(ref el, y - 1, x);
            if (x > 0)
                visitor.Visit(ref Unsafe.Subtract(ref el, 1), y - 1, x - 1);
            if (x < map.Width - 2)
                visitor.Visit(ref Unsafe.Add(ref el, 1), y - 1, x + 1);
        }
        
        if (x > 0)
            visitor.Visit(ref Unsafe.Subtract(ref middle, 1), y, x - 1);
        if (x < map.Width - 2)
            visitor.Visit(ref Unsafe.Add(ref middle, 1), y, x + 1);
        
        if (y < map.Height - 1)
        {
            ref var el = ref Unsafe.Add(ref middle, map.Width);
            visitor.Visit(ref el, y + 1, x);
            if (x > 0)
                visitor.Visit(ref Unsafe.Subtract(ref el, 1), y + 1, x - 1);
            if (x < map.Width - 2)
                visitor.Visit(ref Unsafe.Add(ref el, 1), y + 1, x + 1);
        }
    }
    
    public static int CountNeighborsMatching<TChar, TFilter>(ref ReadOnlySpan2D<TChar> map, TFilter filter, int y, int x)
        where TFilter : struct, ITileFilter<TChar>, allows ref struct
    {
        var sum = 0;
        ref var middle = ref Unsafe.AsRef(in map[y, x]);
        if (y > 0)
        {
            ref var el = ref Unsafe.Subtract(ref middle, map.Width);
            sum += filter.Matches(el) ? 1 : 0;
            if (x > 0)
                sum += filter.Matches(Unsafe.Subtract(ref el, 1)) ? 1 : 0;
            if (x < map.Width - 2)
                sum += filter.Matches(Unsafe.Add(ref el, 1)) ? 1 : 0;
        }
        
        if (x > 0)
            sum += filter.Matches(Unsafe.Subtract(ref middle, 1)) ? 1 : 0;
        if (x < map.Width - 2)
            sum += filter.Matches(Unsafe.Add(ref middle, 1)) ? 1 : 0;
        
        if (y < map.Height - 1)
        {
            ref var el = ref Unsafe.Add(ref middle, map.Width);
            sum += filter.Matches(el) ? 1 : 0;
            if (x > 0)
                sum += filter.Matches(Unsafe.Subtract(ref el, 1)) ? 1 : 0;
            if (x < map.Width - 2)
                sum += filter.Matches(Unsafe.Add(ref el, 1)) ? 1 : 0;
        }

        return sum;
    }
}