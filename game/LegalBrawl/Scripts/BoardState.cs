using Godot;
using System;
using System.Collections.Generic;

public struct BoardState
{
    private PlayerState[] _players;
    public PlayerState this[int index] { get => _players[index]; set => _players[index] = value; }

    public BoardState(PlayerState player, PlayerState opponent)
    {
        _players = new PlayerState[Enum.GetNames(typeof(PlayerTypes)).Length];

        _players[(int)PlayerTypes.Player] = player;
        _players[(int)PlayerTypes.Opponent] = opponent;
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

public struct PlayerState
{
    public int Credibility { get; private set; }
    public List<int> Evidence { get; private set; }
    public List<int> Witnesses { get; private set; }
    public PlayerState(int credibility)
    {
        Credibility = Math.Max(credibility, 0);
        Evidence = new List<int>();
        Witnesses = new List<int>();
    }

    public void AddCredibility(int value)
    {
        Credibility = Math.Max(Credibility + value, 0);
    }
    public void AddEvidence(int value)
    {
        Evidence = new List<int>(Evidence);
        Evidence.Add(value);
    }
    public void AddWitness(int value)
    {
        Witnesses = new List<int>(Witnesses);
        Witnesses.Add(value);
    }
}