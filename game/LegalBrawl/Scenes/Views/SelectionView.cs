using Godot;

public class SelectionView : View
{
    [Signal]
    public delegate void DisplayCards(int[] cardIds, int handSize);
    [Signal]
    public delegate void AddCard(int id);
    [Signal]
    public delegate void RemoveCard(int id);

    public Button HelpButton;
    public Button VolumeButton;
    public Button BattleButton;
    public CardPool CardPool;
    public CardHand CardHand;
    public Button RerollButton;
    private Label _costLabel;

    public override void _Ready()
    {
        base._Ready();

        HelpButton = GetNode<Button>("HelpButton");
        VolumeButton = GetNode<Button>("VolumeButton");
        BattleButton = GetNode<Button>("BattleButton");
        CardPool = GetNode<CardPool>("CardPool");
        CardHand = GetNode<CardHand>("CardHand");
        RerollButton = GetNode<Button>("RerollButton");
        _costLabel = FindNode("CostLabel") as Label;
    }
}