using Godot;
using System;
using System.Collections.Generic;

public class BaseCard : Resource
{
    [Export]
    public int Id;
    [Export]
    public string Name;
    [Export]
    public int Cost;
    [Export]
    public Texture Art;
    [Export]
    public Dictionary<int, int> Effects;
}
