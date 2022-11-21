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

    public Battle.Outcomes GetOutcome()
    {
        int result = _players[0].Credibility - _players[1].Credibility;
        if (result > 0)
            return Battle.Outcomes.Win;
        if (result < 0)
            return Battle.Outcomes.Loss;
        return Battle.Outcomes.Draw;
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