using Godot;
using System.Collections.Generic;

public class Selection : Phase
{
    const int MAX_HAND = 7;
    const int MAX_POOL = 10;
    private List<int> _poolCards;
    private List<int> _handCards;
    private SelectionUI _ui;

    public override void _Ready()
    {
        _poolCards = new List<int>();
        _handCards = new List<int>();
    }

    public void ConnectTo(SelectionUI ui)
    {
        _ui = ui;
        ui.RerollButton.Connect("pressed", this, "OnReroll");
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
        DisplayPool();
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

    public void DisplayPool()
    {
        _ui.CardPool.Display(_poolCards.ToArray());
    }
}