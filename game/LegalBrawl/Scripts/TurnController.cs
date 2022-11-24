using Godot;
using System;
using System.Collections.Generic;

public class TurnController
{
    private Hand[] _hands;
    private int _counter;
    private List<Turn> _turns;

    public TurnController(Hand playerHand, Hand opponentHand)
    {
        _hands = new Hand[Enum.GetNames(typeof(PlayerTypes)).Length];

        _hands[(int)PlayerTypes.Player] = playerHand;
        _hands[(int)PlayerTypes.Opponent] = opponentHand;

        _counter = 0;

        GenerateTurns();
    }

    public void GenerateTurns()
    {
        PlayerTypes first = GetFirstPlayer();
        PlayerTypes second = first == PlayerTypes.Player ? PlayerTypes.Opponent : PlayerTypes.Player;
        _turns = new List<Turn>();

        for (int i = 0; i < Math.Min(_hands[0].Size(), _hands[1].Size()); i++)
        {
            _turns.Add(new Turn(_hands[(int)first].GetIds()[i], first, i));
            _turns.Add(new Turn(_hands[(int)second].GetIds()[i], second, i));
        }
    }

    public PlayerTypes GetFirstPlayer()
    {
        GD.Print(_hands[1]);
        if (_hands[(int)PlayerTypes.Player].FirstCard().Cost >
            _hands[(int)PlayerTypes.Opponent].FirstCard().Cost)
            return PlayerTypes.Player;
        return PlayerTypes.Opponent;
    }

    public Turn Current() => _turns[_counter];

    public void Advance() => _counter++;

    public bool IsLast() => _counter == _turns.Count - 1;

}