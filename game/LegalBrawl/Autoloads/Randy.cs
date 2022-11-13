using Godot;
using System;

public class Randy : Node
{
    private static RandomNumberGenerator _random;

    public override void _Ready()
    {
        _random = new RandomNumberGenerator();
        _random.Randomize();
    }

    public static int Range(int from, int to)
    {
        return _random.RandiRange(from, to);
    }

    public static float Range(float from, float to)
    {
        return _random.RandfRange(from, to);
    }

    public static Vector2 Vector(float from, float to)
    {
        return new Vector2(_random.RandfRange(from, to), _random.RandfRange(from, to));
    }
}