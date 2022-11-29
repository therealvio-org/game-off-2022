using Godot;
using System.Collections.Generic;

public class Selection : Phase
{
    [Signal] public delegate void UpdateFunds(int previous, int current, bool check);
    [Signal] public delegate void UpdateHand(int size, bool check);
    private int _handSize { get => _handCards.Count; }
    private List<int> _poolCards;
    private List<int> _handCards;
    private int _funds;
    private SelectionView _view;

    public bool CheckHand { get => _handCards.Count == Main.MAX_HAND; }
    public bool CheckFunds { get => _funds >= 0; }

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
        view.Connect("Return", this, "OnReturn");
        view.Connect("AddCard", this, "AddToHand");
        view.Connect("RemoveCard", this, "RemoveFromHand");
        Connect("UpdateFunds", view, "OnUpdateFunds");
        Connect("UpdateHand", view, "OnUpdateHand");
    }

    public void OnReroll()
    {
        RollCards();
    }

    public void OnCardsRearranged(int[] cards)
    {
        _handCards = new List<int>(cards);
    }

    public void OnBattle()
    {
        if (CanFight())
        {
            HandCache.Store(new Hand(GetHand(), GameStats.Player.PlayerName));
            EmitSignal("NextPhase", PhaseTypes.Networking);
        }
    }

    public void OnReturn()
    {
        EmitSignal("NextPhase", PhaseTypes.Menu);
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
        while (_poolCards.Count < Main.MAX_POOL)
            _poolCards.Add(CardLibrary.DrawRandomId());
    }

    public void AddToHand(int id)
    {
        // GD.Print("Adding ", CardLibrary.Get(id).Name);
        _handCards.Add(id);
        EmitSignal("UpdateHand", _handCards.Count, CheckHand);
        _funds = CalculateFunds();
    }

    public void RemoveFromHand(int id)
    {
        // GD.Print("Removing ", CardLibrary.Get(id).Name);
        _handCards.Remove(id);
        EmitSignal("UpdateHand", _handCards.Count, CheckHand);
        _funds = CalculateFunds();
    }

    public int[] GetHand()
    {
        //return _handCards.ToArray(); // Ideally phases shouldn't even store references to their associated views
        return _view.GetCardOrder(); // Read comments on this method to understand more
    }

    public int CalculateFunds()
    {
        int funds = Main.STARTING_FUNDS;

        foreach (int cardId in _handCards)
        {
            funds -= CardLibrary.Get(cardId).Cost;
        }

        EmitSignal("UpdateFunds", _funds, funds, funds >= 0);

        return funds;
    }

    public bool CanFight()
    {
        return CheckHand && CheckFunds;
    }
}