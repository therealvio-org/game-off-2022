using Godot;
using System;

public class PlayerStats : Resource
{
    public const string LOCATION = "user://player.tres";

    [Export] public string PlayerId;
    [Export] public string PlayerName = "";
    [Export] public int Wins = 0;
    [Export] public int Losses = 0;

    public void UpdateName(string name)
    {
        PlayerName = name;
        ResourceSaver.Save(LOCATION, this);
    }

    public void AddWin()
    {
        Wins++;
        ResourceSaver.Save(LOCATION, this);
    }

    public void AddLoss()
    {
        Losses++;
        ResourceSaver.Save(LOCATION, this);
    }
}