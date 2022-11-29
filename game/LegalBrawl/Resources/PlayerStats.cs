using Godot;
using System;

public class PlayerStats : Resource
{
    public const string LOCATION = "user://player.tres";

    [Export] public string PlayerId;
    [Export] public string PlayerName = "";
    [Export] public int Wins = 0;
    [Export] public int Losses = 0;
    [Export] public bool FirstTime = true;

    public void UpdateName(string name)
    {
        PlayerName = name;
        ResourceSaver.Save(LOCATION, this);
    }

    public void AddWin()
    {
        Wins++;
        ResourceSaver.Save(LOCATION, this);
        FirstTime = false;
    }

    public void AddLoss()
    {
        Losses++;
        ResourceSaver.Save(LOCATION, this);
        FirstTime = false;
    }

    public void Reset()
    {
        PlayerId = Guid.NewGuid().ToString();
        PlayerName = "";
        Wins = 0;
        Losses = 0;
        FirstTime = true;
        ResourceSaver.Save(LOCATION, this);
    }
}