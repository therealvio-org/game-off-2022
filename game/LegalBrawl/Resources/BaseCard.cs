using Godot;
using System;

public class BaseCard : Resource
{
    [Export]
    public string Name;
    [Export]
    public int Cost;
    [Export]
    public Image Art;
    [Export]
    public string Description;

    public virtual void OnPlay() { }
}
