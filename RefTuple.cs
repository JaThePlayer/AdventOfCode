namespace AoC;

public static class RefTuple
{
    public static RefTuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2)
        where T1 : allows ref struct
        where T2 : allows ref struct
        => new(item1, item2);
}

public ref struct RefTuple<T1, T2>(T1 item1, T2 item2)
    where T1 : allows ref struct
    where T2 : allows ref struct
{
    public T1 Item1 = item1;
    public T2 Item2 = item2;

    public void Deconstruct(out T1 item1, out T2 item2)
    {
        item1 = Item1;
        item2 = Item2;
    }
}