using Godot;
using System;
using System.Collections.Generic;

public class Music : Node
{
    [Export] public AudioStream Menu;
    [Export] public AudioStream Discovery;
    [Export] public AudioStream BattleIntro;
    [Export] public AudioStream Battle;
    private Dictionary<string, AudioStream> _tracks;
    private AudioStreamPlayer2D _mainPlayer;
    private AudioStreamPlayer2D _fadePlayer;
    private AnimationPlayer _animation;


    private bool _debugFade;

    public override void _Ready()
    {
        _mainPlayer = GetNode<AudioStreamPlayer2D>("MusicMainPlayer");
        _fadePlayer = GetNode<AudioStreamPlayer2D>("MusicFadePlayer");
        _animation = GetNode<AnimationPlayer>("AnimationPlayer");

        _tracks = new Dictionary<string, AudioStream>();
        _tracks.Add("Menu", Menu);
        _tracks.Add("Discovery", Discovery);
        _tracks.Add("BattleIntro", BattleIntro);
        _tracks.Add("Battle", Battle);

        _mainPlayer.Stream = _tracks["Menu"];
        _mainPlayer.Play();

        _debugFade = false;

        Debugger.Add("DebugFader", this);
    }

    public void DebugFader()
    {
        if (_debugFade)
            SwitchTo("Menu");
        else
            SwitchTo("Discovery");

        _debugFade = !_debugFade;
    }

    public void SwitchTo(string track)
    {
        _fadePlayer.Stream = _mainPlayer.Stream;
        _fadePlayer.Play(_mainPlayer.GetPlaybackPosition());
        _mainPlayer.Stream = _tracks[track];
        _mainPlayer.Play();
        _animation.Play("Fade");
    }

    public void OnSetVolume(int volume)
    {

    }
}
