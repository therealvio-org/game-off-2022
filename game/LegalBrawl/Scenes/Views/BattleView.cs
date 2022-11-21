using Godot;
using System;

public class BattleView : View
{
    [Signal] public delegate void NextCard();
    [Signal] public delegate void FinishBattle();
    [Signal] public delegate void PlayAgain();

    private Lawyer _player;
    private Lawyer _opponent;
    private Button _nextButton;
    private AnimationPlayer _animator;
    private Label _winnerLabel;
    private Button _againButton;
    public override void _Ready()
    {
        base._Ready();

        _player = FindNode("Player") as Lawyer;
        _player.Represents(PlayerTypes.Player);

        _opponent = FindNode("Opponent") as Lawyer;
        _opponent.Represents(PlayerTypes.Opponent);

        _nextButton = FindNode("NextButton") as Button;
        _nextButton.Connect("pressed", this, "OnNextPressed");

        _againButton = FindNode("AgainButton") as Button;
        _againButton.Connect("pressed", this, "OnAgainPressed");

        _animator = GetNode<AnimationPlayer>("AnimationPlayer");
        _winnerLabel = FindNode("WinnerLabel") as Label;
    }

    public override void Setup()
    {
        _nextButton.Show();
        _winnerLabel.Hide();
        _againButton.Hide();
    }

    public void OnNextPressed()
    {
        EmitSignal("NextCard");
    }
    public void OnAgainPressed()
    {
        EmitSignal("PlayAgain");
    }

    public void OnPlayCard(int cardId, PlayerTypes character, Battle battle)
    {
        GetLawyer(character).Play(cardId, battle);
    }

    public void OnCredibilityChange(PlayerTypes character, int from, int to)
    {
        GetLawyer(character).UpdateCredibility(from, to);
    }

    public void OnLastCard()
    {
        _nextButton.Hide();
        _animator.Play("Wait");
    }

    public void OnDeclareWinner(Battle.Outcomes outcome)
    {
        string output = "";
        switch (outcome)
        {
            case Battle.Outcomes.Win:
                output = "You win!"; break;
            case Battle.Outcomes.Loss:
                output = "Opposing council wins!"; break;
            case Battle.Outcomes.Draw:
                output = "It's a draw!"; break;
        }
        _winnerLabel.Text = output;
        _winnerLabel.Show();
        _againButton.Show();
    }

    public void CallFinishBattle()
    {
        EmitSignal("FinishBattle");
    }

    public Lawyer GetLawyer(PlayerTypes character)
    {
        if (character == PlayerTypes.Player)
            return _player;
        return _opponent;
    }
}
