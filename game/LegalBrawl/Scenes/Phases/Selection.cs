using Godot;
using System.Collections.Generic;

public class Selection : Phase
{
    [Signal] public delegate void UpdateFunds(int previous, int current);
    [Signal] public delegate void UpdateHand(int size);
    const int MAX_HAND = 7;
    const int MAX_POOL = 10;
    const int STARTING_FUNDS = 100;
    private int _handSize { get => _handCards.Count; }
    private List<int> _poolCards;
    private List<int> _handCards;
    private int _funds;
    private SelectionView _view;

    public override void _Ready()
    {
        _poolCards = new List<int>();
        _handCards = new List<int>();
        _funds = CalculateFunds();
    }

    public void ConnectTo(SelectionView view)
    {
        _view = view;
        view.Connect("Reroll", this, "OnReroll");
        view.Connect("Battle", this, "OnBattle");
        view.Connect("AddCard", this, "AddToHand");
        view.Connect("RemoveCard", this, "RemoveFromHand");
        Connect("UpdateFunds", view, "OnUpdateFunds");
        Connect("UpdateHand", view, "OnUpdateHand");
    }

    public void OnReroll()
    {
        RollCards();
    }

    public void OnBattle()
    {
        if (CanFightStart())
        {
            EmitSignal("NextPhase", PhaseTypes.Battle);
        }
    }

    public void RollCards()
    {
        ClearPoolCards();
        foreach (int i in _handCards)
            _poolCards.Add(i);
        RefillPool();
        _view.EmitSignal("DisplayCards", _poolCards, _handSize);
    }

    public void ClearPoolCards()
    {
        _poolCards = new List<int>();
    }

    public void RefillPool()
    {
        while (_poolCards.Count < MAX_POOL)
            _poolCards.Add(CardLibrary.DrawRandomId());
    }

    public void AddToHand(int id)
    {
        GD.Print("Adding ", CardLibrary.Get(id).Name);
        _handCards.Add(id);
        EmitSignal("UpdateHand", _handCards.Count);
        _funds = CalculateFunds();
    }

    public void RemoveFromHand(int id)
    {
        GD.Print("Removing ", CardLibrary.Get(id).Name);
        _handCards.Remove(id);
        EmitSignal("UpdateHand", _handCards.Count);
        _funds = CalculateFunds();
    }

    public int[] GetHand()
    {
        return _handCards.ToArray();
    }

    public int CalculateFunds()
    {
        int funds = STARTING_FUNDS;

        foreach (int cardId in _handCards)
        {
            funds -= CardLibrary.Get(cardId).Cost;
        }

        EmitSignal("UpdateFunds", _funds, funds);

        return funds;
    }

    public bool CanFightStart()
    {
        return _handCards.Count == MAX_HAND && _funds >= 0;
    }
}