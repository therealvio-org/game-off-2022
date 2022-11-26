using Godot;
using System.Collections.Generic;

public class AudioManager : Node
{
    public const float MIN_VOLUME = -60;

    [Signal] public delegate void SetVolume(float volume);
    [Signal] public delegate void SetVolumeDelta(float delta);
    private static AudioManager _instance;
    private Dictionary<string, AudioStreamPlayer> _players;
    private Music _music;

    public override void _Ready()
    {
        _instance = this;
        _players = new Dictionary<string, AudioStreamPlayer>();
        _music = GetNode<Music>("Music");

        Connect("SetVolumeDelta", this, "OnSetVolumeDelta");

        Connect("SetVolume", this, "OnSetVolume");
        _music.Connect("SetVolume", this, "OnSetVolume");

        foreach (Node n in GetChildren())
        {
            if (n is AudioStreamPlayer player)
            {
                _players.Add(n.Name, player);
            }
        }
    }

    public static void Play(string effect)
    {
        _instance._players[effect].Play();
    }

    public static void PlayMusic(string track)
    {
        _instance._music.SwitchTo(track);
    }

    public void OnSetVolumeDelta(float delta)
    {
        float volume = Mathf.Lerp(MIN_VOLUME, 0, Mathf.Clamp(delta, 0, 1));
        EmitSignal("SetVolume", volume);
    }

    public void OnSetVolume(float volume)
    {
        foreach (var kvp in _players)
        {
            kvp.Value.VolumeDb = volume;
        }
    }
}
