namespace AoC;

public interface IConstBool : IConst<bool>
{
}

public interface IConst<T> where T : struct
{
    public static abstract T Value { get; }
}

public struct True : IConstBool
{
    public static bool Value => true;
}

public struct False : IConstBool
{
    public static bool Value => false;
}