using Godot;
using System;

public class MenuView : View
{
    [Signal] public delegate void Play();
    private Button _playButton;
    private Button _leaderboardButton;
    public override void _Ready()
    {
        base._Ready();
        _playButton = FindNode("PlayButton") as Button;
        _playButton.Connect("pressed", this, "OnPlayClicked");
        _leaderboardButton = FindNode("LeaderboardButton") as Button;
        _leaderboardButton.Disabled = true;
    }

    public void OnPlayClicked()
    {
        EmitSignal("Play");
    }
}