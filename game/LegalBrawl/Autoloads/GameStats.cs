using Godot;
using System;

public class GameStats : Node
{
    private static GameStats _instance;
    private string _playerId;

    public override void _Ready()
    {
        _instance = this;
        _playerId = Guid.NewGuid().ToString();
    }

    public static string GetId() => _instance._playerId;
}