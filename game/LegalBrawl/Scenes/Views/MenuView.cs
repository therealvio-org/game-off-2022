using Godot;
using System;

public class MenuView : View
{
    [Signal] public delegate void Play();
    [Signal] public delegate void Tutorial();
    private Button _playButton;
    private Button _leaderboardButton;
    private Button _helpButton;
    private Button _settingsButton;
    public override void _Ready()
    {
        base._Ready();

        _playButton = FindNode("PlayButton") as Button;
        _playButton.Connect("pressed", this, "OnPlayClicked");

        _leaderboardButton = FindNode("LeaderboardButton") as Button;
        _leaderboardButton.Disabled = true;

        _helpButton = FindNode("HelpButton") as Button;
        _helpButton.Connect("pressed", this, "OnHelpClicked");

        _settingsButton = FindNode("SettingsButton") as Button;
        _settingsButton.Disabled = true;
    }

    public void OnPlayClicked()
    {
        EmitSignal("Play");
    }

    public void OnHelpClicked()
    {
        EmitSignal("Tutorial");
    }
}