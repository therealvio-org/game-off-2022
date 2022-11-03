using Godot;
using System;
using System.Collections.Generic;

public class CardHand : Control
{
    [Export]
    public float CardWidth;
    [Export]
    public float ShiftRatio;
    [Export]
    public float CardRotation;
    [Export]
    public float LiftRatio;
    List<Card> _cards;
    Control _cardAnchor;
    bool keyPress = false;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _cardAnchor = GetNode<Control>("Anchor");
        _cards = new List<Card>();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (Input.IsPhysicalKeyPressed((int)Godot.KeyList.A))
        {
            if (!keyPress)
            {
                keyPress = true;
                Card card = SceneManager.Create<Card>(SceneManager.Scenes.Card, this);
                _cards.Add(card);
                AssignCardPositions();
            }
        }
        else if (Input.IsPhysicalKeyPressed((int)Godot.KeyList.D))
        {
            if (!keyPress)
            {
                keyPress = true;
                _cards[_cards.Count - 1].Free();
                _cards.RemoveAt(_cards.Count - 1);
                AssignCardPositions();
            }
        }
        else
            keyPress = false;
    }

    public CardPosition[] CalculateCardPositions()
    {
        float cardShift = CardWidth * ShiftRatio;
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
            _cards[i].SetFixedPosition(positions[i]);
        }
    }
}