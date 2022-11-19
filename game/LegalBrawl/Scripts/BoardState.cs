using Godot;
using System;
using System.Collections.Generic;

public class BoardState
{
    private PlayerState[] _players;

    public BoardState()
    {
        _players = new PlayerState[Enum.GetNames(typeof(PlayerTypes)).Length];

        _players[(int)PlayerTypes.Player] = new PlayerState();
        _players[(int)PlayerTypes.Opponent] = new PlayerState();
    }


    public int GetCredibility(PlayerTypes character)
    {
        return _players[(int)character].Credibility;
    }

    public void UpdateCredibility(PlayerTypes character, int value)
    {
        _players[(int)character].UpdateCredibility(value);
    }

    public PlayerTypes GetWinner()
    {
        if (_players[0].Credibility > _players[1].Credibility)
            return PlayerTypes.Opponent;
        return PlayerTypes.Player;
    }
}

public class PlayerState
{
    public int Credibility { get; private set; } = Main.CREDIBILITY;

    public void UpdateCredibility(int value)
    {
        Credibility = Math.Max(Credibility + value, 0);
    }
}