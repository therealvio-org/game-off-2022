using Godot;

public class SelectionView : View
{

    [Signal] public delegate void DisplayCards(int[] cardIds, int handSize);
    [Signal] public delegate void AddCard(int id);
    [Signal] public delegate void RemoveCard(int id);
    [Signal] public delegate void Reroll();
    [Signal] public delegate void Battle();
    [Signal] public delegate void ShowTips();
    [Signal] public delegate void HideTips();
    [Signal] public delegate void CardHeld(int id);
    [Signal] public delegate void CardDropped(int id, Vector2 location);

    private Button _rerollButton;
    private Button _battleButton;
    private Label _fundsLabel;
    private Label _countLabel;
    private int fundsValue;
    private int realFundsValue;

    public override void _Ready()
    {
        base._Ready();

        _fundsLabel = FindNode("FundsLabel") as Label;
        _countLabel = FindNode("CountLabel") as Label;
        _rerollButton = FindNode("RerollButton") as Button;
        _rerollButton.Connect("pressed", this, "OnRerollClicked");
        _battleButton = FindNode("BattleButton") as Button;
        _battleButton.Connect("pressed", this, "OnBattleClicked");

        Connect("Activate", this, "OnRerollClicked");
    }

    public override void Setup()
    {
        fundsValue = realFundsValue = Selection.STARTING_FUNDS;
        _fundsLabel.Text = FormatFunds(fundsValue);
        _countLabel.Text = FormatCount(0);
        EmitSignal("Reroll");
    }

    public override void _Process(float delta)
    {
        if (fundsValue > realFundsValue)
            fundsValue -= 1;
        if (fundsValue < realFundsValue)
            fundsValue += 1;

        _fundsLabel.Text = FormatFunds(fundsValue);
    }

    public void OnUpdateFunds(int previous, int current, bool check)
    {
        realFundsValue = current;
        _fundsLabel.SelfModulate = check ? Colors.White : Colors.Red;

    }

    public void OnRerollClicked()
    {
        EmitSignal("Reroll");
    }

    public void OnBattleClicked()
    {
        EmitSignal("Battle");
    }

    public void OnUpdateHand(int size, bool check)
    {
        _countLabel.Text = FormatCount(size);
        _countLabel.SelfModulate = check ? Colors.White : Colors.Red;
    }

    // This is a quick fix to get the order of cards from the CardList object on the CardHand
    // Which is possessing state it really shouldn't have
    // I really don't want to have to drill down through all these classes for something
    // That should be represented at the highest level but alas
    public int[] GetCardOrder()
    {
        CardHand cardHand = FindNode("CardHand") as CardHand;
        return cardHand.GetCardOrder();
    }

    public string FormatCount(int size) => $"{size}/7";
    public string FormatFunds(int value) => $"${value}k";
}