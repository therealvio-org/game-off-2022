using Godot;
using System;

public class Debugger : Node
{
    [Export] public bool EnableDebug;
    [Export] public int[] DebugHand;
    private static Debugger _instance;
    public static bool IsDebugMode { get => Exists() && _instance.EnableDebug; }

    public override void _Ready()
    {
        _instance = this;
    }

    public static void Add(string signal, Node owner)
    {
        if (!IsDebugMode)
            return;

        Button button = new Button();
        _instance.AddChild(button);
        button.Text = signal;
        button.Connect("pressed", owner, signal);
    }

    public static Hand GetDebugHand()
    {
        if (_instance.DebugHand.Length == 0)
            return Hand.GetRandom();
        return new Hand(_instance.DebugHand, "Debug Hand");
    }

    public static bool Exists()
    {
        return _instance != null;
    }
}
