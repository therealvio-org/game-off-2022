using Godot;
using System;

public class BattleView : View
{
    [Signal] public delegate void NextCard();

    private Lawyer _player;
    private Lawyer _opponent;

    public override void _Ready()
    {
        GetNode<Button>("NextButton").Connect("pressed", this, "OnNextPressed");
        _player = GetNode<Lawyer>("Player");
        _opponent = GetNode<Lawyer>("Opponent");
    }

    public void OnNextPressed()
    {
        EmitSignal("NextCard");
    }

    public void OnPlayCard(int cardId, Lawyer.Character character)
    {
        if (character == Lawyer.Character.Player)
            _player.Play(cardId);
        else
            _opponent.Play(cardId);
    }
}
