using Godot;
using System;

public class MenuView : View
{
    [Signal] public delegate void Play();
    private Button _playButton;
    public override void _Ready()
    {
        base._Ready();
        _playButton = FindNode("PlayButton") as Button;
        _playButton.Connect("pressed", this, "OnPlayClicked");
    }

    public void OnPlayClicked()
    {
        EmitSignal("Play");
    }
}