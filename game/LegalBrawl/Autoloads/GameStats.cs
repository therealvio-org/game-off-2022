using Godot;
using System;

public class GameStats : Node
{
    private static GameStats _instance;
    private string _playerId;

    public override void _Ready()
    {
        _instance = this;
        _playerId = "6d0506fc-c6bf-4dd6-a3ec-cca4515990d1";
    }

    public static string GetId() => _instance._playerId;
}