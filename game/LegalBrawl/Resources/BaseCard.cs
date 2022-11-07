using Godot;
using System;

public class BaseCard : Resource
{
    [Export]
    public int Id;
    [Export]
    public string Name;
    [Export]
    public int Cost;
    [Export]
    public Image Art;
    [Export]
    public string Description;

    protected Player _self;
    protected Player _opponent;

    public virtual void OnPlay() { }

    public void Initialise(Player self, Player opponent)
    {
        _self = self;
        _opponent = opponent;
    }
}
