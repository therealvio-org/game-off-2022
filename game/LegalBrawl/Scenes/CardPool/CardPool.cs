using Godot;
using System;
using System.Collections.Generic;

public class CardPool : Control
{
    private List<CardDisplay> _displayCards;
    private int _nextFlipId;
    private bool _flipToFace;
    private Control _dropTip;

    private AnimationPlayer _animationPlayer;

    public override void _Ready()
    {
        _displayCards = new List<CardDisplay>();
        _animationPlayer = FindNode("AnimationPlayer") as AnimationPlayer;
        _dropTip = FindNode("DropTip") as Control;

        foreach (Node n in GetNode("Column/TopRow").GetChildren()) _displayCards.Add(n as CardDisplay);
        foreach (Node n in GetNode("Column/BottomRow").GetChildren()) _displayCards.Add(n as CardDisplay);

        foreach (CardDisplay c in _displayCards)
        {
            c.FlipDown();
            c.Connect("Click", this, "OnCardClicked");
            c.Connect("mouse_entered", c, "OnHoverStart");
            c.Connect("mouse_exited", c, "OnHoverEnd");
        }

        Owner.Connect("DisplayCards", this, "OnDisplay");
        Owner.Connect("Enter", this, "OnEnter");
        Owner.Connect("Exit", this, "OnExit");
        Owner.Connect("CardHeld", this, "OnCardHeld");
        Owner.Connect("CardDropped", this, "OnCardDropped");
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

    public void OnCardClicked(CardDisplay card)
    {
        if (card.IsFlipped)
            return;

        if (card.IsSelected)
        {
            Owner.EmitSignal("RemoveCard", card.Id);
            card.Unselect();
        }
        else
        {
            Owner.EmitSignal("AddCard", card.Id);
            card.Select();
        }
    }

    public void OnCardHeld(int id)
    {
        _dropTip.Show();
    }

    public void OnCardDropped(int id, Vector2 location)
    {
        if (_dropTip.GetGlobalRect().HasPoint(location))
        {
            Owner.EmitSignal("RemoveCard", id);
            _displayCards.Find((c) => c.Id == id && c.IsSelected).Unselect();
        }

        _dropTip.Hide();
    }
}
