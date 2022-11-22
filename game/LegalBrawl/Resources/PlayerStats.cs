using Godot;
using System;

public class PlayerStats : Resource
{
    public const string LOCATION = "user://player.tres";

    [Export] public string PlayerId;
    [Export] public string PlayerName = "";
    [Export] public int Wins;
    [Export] public int Losses;

    public void UpdateName(string name)
    {
        PlayerName = name;
        ResourceSaver.Save(LOCATION, this);
    }
}