using Godot;
using System.Collections.Generic;

public class Selection : Phase
{
    const int MAX_HAND = 7;
    const int MAX_POOL = 10;
    private int _handSize { get => _handCards.Count; }
    private List<int> _poolCards;
    private List<int> _handCards;
    private SelectionView _view;

    public override void _Ready()
    {
        _poolCards = new List<int>();
        _handCards = new List<int>();
    }

    public void ConnectTo(SelectionView view)
    {
        _view = view;
        view.RerollButton.Connect("pressed", this, "OnReroll");
        view.Connect("AddCard", this, "AddToHand");
        view.Connect("RemoveCard", this, "RemoveFromHand");
    }

    public void OnReroll()
    {
        RollCards();
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
    }

    public void RemoveFromHand(int id)
    {
        GD.Print("Removing ", CardLibrary.Get(id).Name);
        _handCards.Remove(id);
    }
}