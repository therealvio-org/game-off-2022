using Godot;
using System;
using System.Collections.Generic;

public class CardHand : Control
{
    [Signal] public delegate void CardHeld(int id);
    [Signal] public delegate void CardDropped(int id, Vector2 location);
    [Export]
    public float CardWidth;
    [Export]
    public float ShiftRatio;
    [Export]
    public float CardRotation;
    [Export]
    public float LiftRatio;
    CardList _cards;
    Control _cardAnchor;
    Control _dragTip;
    bool keyPress = false;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _cardAnchor = GetNode<Control>("Anchor");
        _dragTip = FindNode("DragTip") as Control;
        _cards = new CardList();
        Owner.Connect("AddCard", this, "OnAdd");
        Owner.Connect("RemoveCard", this, "OnRemove");
        Owner.Connect("Enter", this, "OnEnter");
        Owner.Connect("Exit", this, "OnExit");
        Connect("CardHeld", this, "OnCardHeld");
        Connect("CardDropped", this, "OnCardDropped");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        //bool assignPositions = false;

        if (_cards.IsHeld && _cards.IsHovered)
        {
            // GD.Print("Held: ", _cards.IsHeld ? _cards.LastHeld.Name : "None");
            // GD.Print("Hover: ", _cards.IsHovered ? _cards.LastHovered.Name : "None");
            if (_cards.LastHeld != _cards.LastHovered && !_cards.LastHovered.IsMoving)
            {
                _cards.MoveHeldToHover();
            }
        }

        // @TODO Make this only assign when necessary
        // if (_cards.IsHeld)
        //     assignPositions = true;


        // if (assignPositions)
        AssignCardPositions();

    }

    public CardPosition[] CalculateCardPositions()
    {
        float cardShift = CardWidth * ShiftRatio * (_cards.IsHeld ? 1.5f : 1);
        float cardLift = CardWidth * LiftRatio;
        int cardCount = _cards.Count;
        CardPosition[] cardPositions = new CardPosition[cardCount];

        float maxWidth = ((cardShift) * cardCount) - cardShift;
        float shiftStep = 0;
        float rotStep = CardRotation * ((cardCount - 1) / 2f);
        Vector2 offset = _cardAnchor.RectGlobalPosition;

        for (int i = 0; i < cardPositions.Length; i++)
        {
            float lift = Mathf.Pow((i - ((cardCount - 1) / 2f)), 2) * cardLift;
            Vector2 point = new Vector2(shiftStep - (maxWidth / 2), lift);
            cardPositions[i] = new CardPosition(point + offset, rotStep);
            shiftStep += cardShift; //step = (cardGap * cardCount) - cardGap * (cardCount / 2)
            rotStep -= CardRotation;
            //GD.Print(point);
        }

        return cardPositions;
    }

    public void AssignCardPositions()
    {
        var positions = CalculateCardPositions();
        for (int i = 0; i < _cards.Count; i++)
        {
            _cards.Get(i).SetFixedPosition(positions[i]);
        }
    }

    public int[] GetCardOrder()
    {
        int[] ids = new int[_cards.Count];
        for (int i = 0; i < _cards.Count; i++)
        {
            ids[i] = _cards.List[i].Display.Resource.Id;
        }
        return ids;
    }


    public void OnAdd(int id)
    {
        Card card = SceneManager.Create<Card>(SceneManager.Scenes.Card, this);
        card.Display.Display(CardLibrary.Get(id));
        _cards.Add(card);
        card.MakeGrabbable();
    }

    public void OnRemove(int id)
    {
        _cards.RemoveId(id);
    }

    public void OnEnter()
    {
        _cards = new CardList();
    }

    public void OnExit()
    {
        _cards.RemoveAll();
    }

    public void OnCardHeld(int id)
    {
        GD.Print("Holding " + CardLibrary.NameOf(id));
        Owner.EmitSignal("CardHeld", id);
        _dragTip.Show();
    }

    public void OnCardDropped(int id, Vector2 location)
    {
        GD.Print("Dropped " + CardLibrary.NameOf(id));
        Owner.EmitSignal("CardDropped", id, location);
        _dragTip.Hide();
    }
}