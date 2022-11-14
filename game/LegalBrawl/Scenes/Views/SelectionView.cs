using Godot;

public class SelectionView : View
{
    [Signal] public delegate void DisplayCards(int[] cardIds, int handSize);
    [Signal] public delegate void AddCard(int id);
    [Signal] public delegate void RemoveCard(int id);
    [Signal] public delegate void Reroll();
    [Signal] public delegate void Battle();

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
    }

    public override void _Process(float delta)
    {
        if (fundsValue > realFundsValue)
            fundsValue -= 1;
        if (fundsValue < realFundsValue)
            fundsValue += 1;

        _fundsLabel.Text = FormatFunds(fundsValue);
    }

    public void OnUpdateFunds(int previous, int current)
    {
        realFundsValue = current;
    }

    public void OnRerollClicked()
    {
        EmitSignal("Reroll");
    }

    public void OnBattleClicked()
    {
        EmitSignal("Battle");
    }

    public void OnUpdateHand(int size)
    {
        _countLabel.Text = $"{size}/7";
    }

    public string FormatFunds(int value) => $"${value}k";
}