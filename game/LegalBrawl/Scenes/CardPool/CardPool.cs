using Godot;
using System;
using System.Collections.Generic;

public class CardPool : Control
{
    private List<CardDisplay> _displayCards;
    private int _nextFlipId;
    private bool _flipToFace;
    private AnimationPlayer _animationPlayer;

    public override void _Ready()
    {
        _displayCards = new List<CardDisplay>();
        _animationPlayer = FindNode("AnimationPlayer") as AnimationPlayer;

        foreach (Node n in GetNode("Column/TopRow").GetChildren()) _displayCards.Add(n as CardDisplay);
        foreach (Node n in GetNode("Column/BottomRow").GetChildren()) _displayCards.Add(n as CardDisplay);

        foreach (CardDisplay c in _displayCards)
        {
            c.FlipDown();
            c.Connect("OnClick", this, "CardClicked");
        }

        Owner.Connect("DisplayCards", this, "OnDisplay");
        Owner.Connect("Enter", this, "OnEnter");
        Owner.Connect("Exit", this, "OnExit");
    }

    public void OnEnter()
    {

    }

    public void OnExit()
    {
        foreach (CardDisplay c in _displayCards)
        {
            c.FlipDown();
            c.Unselect();
        }
    }

    public void OnDisplay(int[] cardIds, int handSize)
    {
        for (int i = 0; i < _displayCards.Count; i++)
        {
            if (i < handSize)
                _displayCards[i].QueueDisplay(CardLibrary.Get(cardIds[i]), false);
            else
                _displayCards[i].QueueDisplay(CardLibrary.Get(cardIds[i]));
        }

        _nextFlipId = 0;
        _flipToFace = true;
        _animationPlayer.Stop(true);
        _animationPlayer.Play("FlipAll");
    }

    public void FlipNext()
    {
        if (_nextFlipId >= _displayCards.Count)
            return;

        if (_flipToFace)
            _displayCards[_nextFlipId++].FlipUp();
        else
            _displayCards[_nextFlipId++].FlipDown();
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
