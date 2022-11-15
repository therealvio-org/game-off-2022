using Godot;
using System;
using System.Collections.Generic;

public class Lawyer : Control
{
    private List<CardDisplay> _cards;
    private CardList _list;
    private Control _handAnchor;
    private Control _playedAnchor;
    private Control _container;
    private Label _credibility;
    private Label _damageText;
    private AnimationPlayer _animator;

    private int _handSize;
    private PlayerTypes _representing;
    public override void _Ready()
    {
        _cards = new List<CardDisplay>();
        _handAnchor = FindNode("Hand") as Control;
        _playedAnchor = FindNode("PlayedAnchor") as Control;
        _container = FindNode("HBoxContainer") as Control;
        _credibility = FindNode("CredibilityLabel") as Label;
        _damageText = FindNode("DamageLabel") as Label;
        _animator = FindNode("AnimationPlayer") as AnimationPlayer;

        foreach (Node n in _handAnchor.GetChildren())
            _cards.Add(n as CardDisplay);

        _list = new CardList();

        Owner.Connect("Enter", this, "Setup");
        Owner.Connect("Exit", this, "Cleanup");
    }

    public void Play(int id, Battle battle)
    {
        _cards[--_handSize].Hide();

        Card card = SceneManager.Create<Card>(SceneManager.Scenes.Card, this);
        CardDisplay display = card.Display;
        display.ShowBack();
        display.FlipUp();
        BaseCard resource = CardLibrary.Get(id);
        resource.Initialise(battle, _representing);
        display.Display(resource);
        _list.Add(card);

        card.SetCardPosition(new CardPosition(_cards[0].RectGlobalPosition + (_cards[0].RectSize / 2), 0f));
        card.SetFixedPosition(new CardPosition(_playedAnchor.RectGlobalPosition + Randy.Vector(-5, 5), Randy.Range(-4f, 4f)));

        display.Prime();
    }

    public void Setup()
    {
        ShowCards();
        _handSize = _cards.Count;
        _credibility.Text = "0";
    }

    public void ShowCards()
    {
        foreach (CardDisplay c in _cards)
        {
            c.Show();
            c.ShowBack();
            c.OffsetRotation();
        }
    }

    public void Cleanup()
    {
        _list.RemoveAll();
    }

    public void FlipDirection()
    {
        _container.MoveChild(_playedAnchor, 0);
    }

    internal void Represents(PlayerTypes character)
    {
        _representing = character;
        if (character == PlayerTypes.Opponent)
            FlipDirection();
    }

    public static PlayerTypes GetOther(PlayerTypes character)
    {
        if (character == PlayerTypes.Player)
            return PlayerTypes.Opponent;
        return PlayerTypes.Player;
    }

    public void UpdateCredibility(int from, int to)
    {
        _credibility.Text = $"{to}";
        _damageText.Text = $"{to - from}";
        if (to > from)
            _animator.Play("CredibilityUp");
        else
            _animator.Play("CredibilityDown");
    }
}
