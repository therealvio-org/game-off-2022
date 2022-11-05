using Godot;
using System;

public class Debugger : Node
{
    private static Debugger _instance;

    public override void _Ready()
    {
        _instance = this;

    }

    public static void Add(string signal, Node owner)
    {
        Button button = new Button();
        _instance.AddChild(button);
        button.Text = signal;
        button.Connect("pressed", owner, signal);
    }
}
