using Godot;
using System;

public class BattleView : View
{
    [Signal] public delegate void NextTurn();
    [Signal] public delegate void FinishBattle();
    [Signal] public delegate void PlayAgain();
    [Signal] public delegate void Menu();

    private Lawyer _player;
    private Lawyer _opponent;
    private Button _nextButton;
    private AnimationPlayer _animator;
    private Label _winnerLabel;
    private Button _againButton;
    private Button _menuButton;
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

        _menuButton = FindNode("MenuButton") as Button;
        _menuButton.Connect("pressed", this, "OnMenuPressed");

        _animator = GetNode<AnimationPlayer>("AnimationPlayer");
        _winnerLabel = FindNode("WinnerLabel") as Label;
    }

    public override void Setup()
    {
        _nextButton.Show();
        _winnerLabel.Hide();
        _againButton.Hide();
        _menuButton.Hide();
    }

    public void OnNextPressed()
    {
        EmitSignal("NextTurn");
    }
    public void OnAgainPressed()
    {
        EmitSignal("PlayAgain");
    }

    public void OnMenuPressed()
    {
        EmitSignal("Menu");
    }

    public void OnPlayTurn(Turn turn)
    {
        // Gives a card to display
        // Makes effects and shit happen
        CardDisplay cardDisplay = GetLawyer(turn.Owner).PlayCard(turn.CardId);
        cardDisplay.Connect("Reveal", this, "OnCardReveal", new Godot.Collections.Array() { turn });
    }

    public void OnCardReveal(Turn turn)
    {
        _player.UpdateState(turn.StartState[(int)PlayerTypes.Player], turn.EndState[(int)PlayerTypes.Player]);
        _opponent.UpdateState(turn.StartState[(int)PlayerTypes.Opponent], turn.EndState[(int)PlayerTypes.Opponent]);
    }

    public void OnLastTurn()
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
                GameStats.Player.AddWin();
                output = $"{HandCache.Get(PlayerTypes.Player).Name} wins!";
                break;
            case Battle.Outcomes.Loss:
                GameStats.Player.AddLoss();
                output = $"{HandCache.Get(PlayerTypes.Opponent).Name} wins!";
                break;
            case Battle.Outcomes.Draw:
                output = "It's a draw!"; break;
        }
        _winnerLabel.Text = output;
        _winnerLabel.Show();
        _againButton.Show();
        _menuButton.Show();
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
