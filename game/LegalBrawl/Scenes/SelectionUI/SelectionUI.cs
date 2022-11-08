using Godot;

public class SelectionUI : Control
{
    [Signal]
    public delegate void DisplayCards(int[] cardIds, int handSize);
    [Signal]
    public delegate void AddCard(int id);
    [Signal]
    public delegate void RemoveCard(int id);
    [Signal]
    public delegate void Enter();
    [Signal]
    public delegate void Exit();


    public Button HelpButton;
    public Button VolumeButton;
    public Button BattleButton;
    public CardPool CardPool;
    public CardHand CardHand;
    public Button RerollButton;

    public override void _Ready()
    {
        HelpButton = GetNode<Button>("HelpButton");
        VolumeButton = GetNode<Button>("VolumeButton");
        BattleButton = GetNode<Button>("BattleButton");
        CardPool = GetNode<CardPool>("CardPool");
        CardHand = GetNode<CardHand>("CardHand");
        RerollButton = GetNode<Button>("RerollButton");

        Connect("Enter", this, "OnEnter");
        Connect("Exit", this, "OnExit");

        HelpButton.Hide();
        VolumeButton.Hide();
        BattleButton.Hide();
        CardPool.Hide();
        CardHand.Hide();
        RerollButton.Hide();
    }

    public void OnEnter()
    {
        HelpButton.Show();
        VolumeButton.Show();
        BattleButton.Show();
        CardPool.Show();
        CardHand.Show();
        RerollButton.Show();
    }

    public void OnExit()
    {
        HelpButton.Hide();
        VolumeButton.Hide();
        BattleButton.Hide();
        CardPool.Hide();
        CardHand.Hide();
        RerollButton.Hide();
    }
}