using Godot;
using System;
using System.Collections.Generic;

public class CardPool : Control
{
    private List<CardDisplay> _displayCards;
    public override void _Ready()
    {
        _displayCards = new List<CardDisplay>();

        foreach (Node n in GetNode("Column/TopRow").GetChildren()) _displayCards.Add(n as CardDisplay);
        foreach (Node n in GetNode("Column/BottomRow").GetChildren()) _displayCards.Add(n as CardDisplay);

        foreach (CardDisplay c in _displayCards)
        {
            c.FlipDown();
            c.Connect("OnClick", this, "CardClicked");
        }

        Owner.Connect("DisplayCards", this, "OnDisplay");
    }

    public void OnDisplay(int[] cardIds, int handSize)
    {
        for (int i = 0; i < _displayCards.Count; i++)
        {
            if (i >= cardIds.Length)
                _displayCards[i].FlipDown();
            else if (i < handSize)
                _displayCards[i].Display(CardLibrary.Get(cardIds[i]), false);
            else
                _displayCards[i].Display(CardLibrary.Get(cardIds[i]));
        }
    }

    public void CardClicked(int id, bool selected, bool flipped)
    {
        if (flipped)
            return;

        if (selected)
        {
            Owner.EmitSignal("RemoveCard", id);
        }
        else
        {
            Owner.EmitSignal("AddCard", id);
        }
    }
}
