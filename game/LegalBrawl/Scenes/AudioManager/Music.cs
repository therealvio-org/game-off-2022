using Godot;
using System;
using System.Collections.Generic;

public class Music : Node
{
    [Export] public AudioStream Menu;
    [Export] public AudioStream Discovery;
    [Export] public AudioStream Crowd;
    [Export] public AudioStream BattleIntro;
    [Export] public AudioStream Battle;
    private Dictionary<string, AudioStream> _tracks;
    private AudioStreamPlayer2D _mainPlayer;
    private AudioStreamPlayer2D _fadePlayer;
    private AnimationPlayer _animation;
    private string _playing;


    private bool _debugFade;

    public override void _Ready()
    {
        _mainPlayer = GetNode<AudioStreamPlayer2D>("MusicMainPlayer");
        _fadePlayer = GetNode<AudioStreamPlayer2D>("MusicFadePlayer");
        _animation = GetNode<AnimationPlayer>("AnimationPlayer");

        _tracks = new Dictionary<string, AudioStream>();
        _tracks.Add("Menu", Menu);
        _tracks.Add("Selection", Discovery);
        _tracks.Add("Crowd", Crowd);
        _tracks.Add("BattleIntro", BattleIntro);
        _tracks.Add("Battle", Battle);

        _mainPlayer.Stream = _tracks["Menu"];
        _mainPlayer.Play();
        _playing = "Menu";
        _mainPlayer.Connect("finished", this, "OnFinished");

        _debugFade = false;

        Debugger.Add("DebugFader", this);
    }

    public void DebugFader()
    {
        if (_debugFade)
            SwitchTo("Menu");
        else
            SwitchTo("Selection");

        _debugFade = !_debugFade;
    }

    public void SwitchTo(string track)
    {
        if (_playing == track)
            return;

        _playing = track;
        if (track == "Battle")
            track = "BattleIntro";

        _fadePlayer.Stream = _mainPlayer.Stream;
        _fadePlayer.Play(_mainPlayer.GetPlaybackPosition());
        _mainPlayer.Stream = _tracks[track];
        _mainPlayer.Play();
        _animation.Play("Fade");
    }

    public void OnFinished()
    {
        _mainPlayer.Stream = _tracks[_playing];
        _mainPlayer.Play();
    }
}
