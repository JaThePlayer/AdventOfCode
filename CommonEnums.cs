using System.Runtime.CompilerServices;

namespace AoC;

[Flags]
public enum Direction : byte
{
    None = 0,
    Right = 1,
    Left = 2,
    Up = 4,
    Down = 8,
}

public struct DirectionRight : IConst<Direction> { public static Direction Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Direction.Right; }}
public struct DirectionLeft : IConst<Direction> { public static Direction Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Direction.Left; }}
public struct DirectionUp : IConst<Direction> { public static Direction Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Direction.Up; }}
public struct DirectionDown : IConst<Direction> { public static Direction Value { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Direction.Down; }}

public interface IDirPicker
{
    public static abstract bool Left { get; }
    public static abstract bool Right { get; }
    public static abstract bool Up { get; }
    public static abstract bool Down { get; }
}

public struct DirPicker<TLeft, TRight, TUp, TDown> : IDirPicker
    where TLeft : IConstBool
    where TRight : IConstBool
    where TUp : IConstBool
    where TDown : IConstBool
{
    public static bool Left => TLeft.Value;
    public static bool Right => TRight.Value;
    public static bool Up => TUp.Value;
    public static bool Down => TDown.Value;
}

public struct ExceptLeftPicker : IDirPicker
{
    public static bool Left => false;
    public static bool Right => true;
    public static bool Up => true;
    public static bool Down => true;
}

public struct ExceptRightPicker : IDirPicker
{
    public static bool Left => true;
    public static bool Right => false;
    public static bool Up => true;
    public static bool Down => true;
}

public struct ExceptDownPicker : IDirPicker
{
    public static bool Left => true;
    public static bool Right => true;
    public static bool Up => true;
    public static bool Down => false;
}

public struct ExceptUpPicker : IDirPicker
{
    public static bool Left => true;
    public static bool Right => true;
    public static bool Up => false;
    public static bool Down => true;
}

public struct AllDirPicker : IDirPicker
{
    public static bool Left => true;
    public static bool Right => true;
    public static bool Up => true;
    public static bool Down => true;
}