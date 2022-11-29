using Godot;
using System;
using System.Collections.Generic;

public class Music : Node
{
    private Dictionary<string, AudioStreamPlayer> _tracks;

    private string _playing = "";

    public override void _Ready()
    {
        _tracks = new Dictionary<string, AudioStreamPlayer>();

        foreach (Node n in GetChildren())
        {
            if (n is AudioStreamPlayer player)
            {
                _tracks.Add(n.Name, player);
                Tween tween = new Tween();
                tween.Name = "Fader";
                player.AddChild(tween);
            }
        }

        _tracks["BattleIntro"].Connect("finished", this, "BattleIntroFinished");
    }

    public void SwitchTo(string track)
    {
        if (_playing == track)
            return;

        if (_playing != "")
            FadeOut(_tracks[_playing]);

        if (track == "Battle")
            track = "BattleIntro";

        _playing = track;
        FadeIn(_tracks[track]);
    }

    public void FadeIn(AudioStreamPlayer _player)
    {
        _player.Play();
        _player.VolumeDb = -30f;
        Tween fader = _player.GetNode<Tween>("Fader");
        fader.InterpolateProperty(_player, "volume_db", -30f, 0f, 1f);
        fader.Start();
    }

    public void FadeOut(AudioStreamPlayer _player)
    {
        Tween fader = _player.GetNode<Tween>("Fader");
        fader.InterpolateProperty(_player, "volume_db", _player.VolumeDb, -100f, 1f, Tween.TransitionType.Quart, Tween.EaseType.In);
        fader.Start();
    }

    public void BattleIntroFinished()
    {
        if (_playing != "BattleIntro")
            return;

        _playing = "Battle";
        _tracks["Battle"].VolumeDb = 0f;
        _tracks["Battle"].Play();
    }
}
