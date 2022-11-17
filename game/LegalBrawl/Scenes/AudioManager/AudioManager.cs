using Godot;
using System.Collections.Generic;

public class AudioManager : Node
{
    [Signal] public delegate void SetVolume(float volume);
    private static AudioManager _instance;
    private AudioStreamPlayer _hoverPlayer;
    private AudioStreamPlayer _clickPlayer;
    private Dictionary<string, AudioStreamPlayer> _players;
    public override void _Ready()
    {
        _instance = this;
        _players = new Dictionary<string, AudioStreamPlayer>();

        _hoverPlayer = GetNode<AudioStreamPlayer>("Hover");
        _clickPlayer = GetNode<AudioStreamPlayer>("Click");

        _players.Add("Hover", _hoverPlayer);
        _players.Add("Click", _clickPlayer);
    }

    public static void Play(string effect)
    {
        _instance._players[effect].Play();
    }
}
