using Godot;
using System;

public class Turn : Godot.Object
{
    public int CardId { get; private set; }
    public BaseCard Card { get => CardLibrary.Get(CardId); }
    public PlayerTypes Owner { get; private set; }
    public BoardState StartState { get; private set; }
    public BoardState EndState { get; private set; }
    public int Round { get; private set; }
    public bool IsComplete { get; private set; }

    public Turn(int id, PlayerTypes owner, int round)
    {
        CardId = id;
        Owner = owner;
        Round = round;
        IsComplete = false;
    }

    public BoardState Perform(BoardState state)
    {
        StartState = state;
        EndState = CardLogic.Apply(state, this);
        return EndState;
    }
}