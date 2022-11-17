using Godot;
using System.Collections.Generic;

public class AudioManager : Node
{
    [Signal] public delegate void SetVolume(float volume);
    private static AudioManager _instance;
    private Dictionary<string, AudioStreamPlayer> _players;
    public override void _Ready()
    {
        _instance = this;
        _players = new Dictionary<string, AudioStreamPlayer>();

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
}
