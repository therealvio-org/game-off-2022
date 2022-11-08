using Godot;
using System;
using System.Collections.Generic;

public class CardPool : Control
{
    private CardList _activeCards;
    private List<CardDisplay> _displayCards;
    public override void _Ready()
    {
        _activeCards = new CardList();
        _displayCards = new List<CardDisplay>();

        foreach (Node n in GetNode("Column/TopRow").GetChildren()) _displayCards.Add(n as CardDisplay);
        foreach (Node n in GetNode("Column/BottomRow").GetChildren()) _displayCards.Add(n as CardDisplay);

        foreach (CardDisplay c in _displayCards) GD.Print(c.Name);
    }

    public void Display(int[] cardIds)
    {
        for (int i = 0; i < cardIds.Length; i++)
        {
            _displayCards[i].Display(CardLibrary.Get(cardIds[i]));
        }
    }
}
