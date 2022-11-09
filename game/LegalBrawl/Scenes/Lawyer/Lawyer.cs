using Godot;
using System;

public class Lawyer : Control
{
    public enum Character { Player, Opponent };
    public override void _Ready()
    {

    }

    public void Play(int id)
    {
        GD.Print(id);
    }
}
