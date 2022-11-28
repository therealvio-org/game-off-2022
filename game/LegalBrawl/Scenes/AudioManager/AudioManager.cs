using Godot;
using System.Collections.Generic;

public class AudioManager : Node
{
    private static AudioManager _instance;
    private Dictionary<string, AudioStreamPlayer> _players;
    private Music _music;

    public override void _Ready()
    {
        _instance = this;
        _players = new Dictionary<string, AudioStreamPlayer>();
        _music = GetNode<Music>("Music");

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

    public static void SetVolumeLevel(string channel, int interval)
    {
        float[] volumeLevels = { -60, -12, -6, -3, 0 };
        SetChannelVolume(channel, volumeLevels[interval]);
    }

    public static void SetChannelVolume(string channel, float volume)
    {
        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex(channel), volume);
    }
}
