using Godot;
using System;

public class BoardState
{
    private PlayerState _player;
    private PlayerState _opponent;

    public BoardState()
    {
        _player = new PlayerState();
        _opponent = new PlayerState();
    }

    public int GetCredibility(PlayerTypes character)
    {
        return GetPlayer(character).Credibility;
    }

    public void UpdateCredibility(PlayerTypes character, int value)
    {
        GetPlayer(character).Credibility += value;
    }

    public PlayerState GetPlayer(PlayerTypes character)
    {
        return character == PlayerTypes.Player ? _player : _opponent;
    }

    public PlayerTypes GetWinner()
    {
        if (_opponent.Credibility > _player.Credibility)
            return PlayerTypes.Opponent;
        return PlayerTypes.Player;
    }
}

public class PlayerState
{
    public int Credibility { get; set; } = 0;
}