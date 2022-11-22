using Godot;
using System;

public class GameStats : Node
{
    private GameSettings _settings;
    private PlayerStats _player;
    private static GameStats _instance;

    public static string PlayerName { get => _instance._player.PlayerName; }

    public override void _Ready()
    {
        _instance = this;

        if (!ResourceLoader.Exists(GameSettings.LOCATION))
        {
            GameSettings settings = GD.Load<CSharpScript>("res://Resources/GameSettings.cs").New() as GameSettings;
            ResourceSaver.Save(GameSettings.LOCATION, settings);
        }

        if (!ResourceLoader.Exists(PlayerStats.LOCATION))
        {
            PlayerStats player = GD.Load<CSharpScript>("res://Resources/PlayerStats.cs").New() as PlayerStats;
            player.PlayerId = Guid.NewGuid().ToString();
            ResourceSaver.Save(PlayerStats.LOCATION, player);
        }

        _settings = GD.Load<GameSettings>(GameSettings.LOCATION);
        _player = GD.Load<PlayerStats>(PlayerStats.LOCATION);
    }

    public static bool HasPlayerName() => _instance._player.PlayerName != "";
    public static void SetPlayerName(string name) => _instance._player.UpdateName(name);
    public static string GetId() => _instance._player.PlayerId;
}