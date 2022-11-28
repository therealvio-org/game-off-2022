using Godot;
using System;

public class NetworkingView : View
{
    private Label _currentAction;
    private AnimationPlayer _animationPlayer;
    public override void _Ready()
    {
        base._Ready();
        _currentAction = FindNode("CurrentAction") as Label;
        _animationPlayer = FindNode("AnimationPlayer") as AnimationPlayer;
        _animationPlayer.Play("Loading");
    }

    public override void Setup()
    {
        _currentAction.Text = "Waiting for network...";
    }

    public void OnSendPlayerData()
    {
        _currentAction.Text = "Connecting to server...";
    }

    public void OnGetOpponentData()
    {
        _currentAction.Text = "Looking for opponent...";
    }

}
