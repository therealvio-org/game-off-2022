using Godot;
using System;

public class Turn : Godot.Object
{
    private TurnController _controller;
    public int CardId { get; private set; }
    public BaseCard Card { get => CardLibrary.Get(CardId); }
    public PlayerTypes Owner { get; private set; }
    public BoardState StartState { get; private set; }
    public BoardState EndState { get; private set; }
    public int Round { get; private set; }
    public int Index { get; private set; }
    public bool IsComplete { get; private set; }
    public bool Skipped { get; private set; }

    public Turn(TurnController controller, int id, PlayerTypes owner, int round, int index)
    {
        _controller = controller;
        CardId = id;
        Owner = owner;
        Round = round;
        Index = index;
        IsComplete = false;
        Skipped = false;
    }

    public BoardState Perform(BoardState state)
    {
        StartState = state;
        EndState = Skipped ? state : CardLogic.Apply(state, this);
        return EndState;
    }

    public Turn Next()
    {
        return _controller.GetTurn(Index + 1);
    }

    public void Skip()
    {
        Skipped = true;
    }
}