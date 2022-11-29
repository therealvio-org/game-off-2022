using Godot;

public class SelectionView : View
{

    [Signal] public delegate void DisplayCards(int[] cardIds, int handSize);
    [Signal] public delegate void AddCard(int id);
    [Signal] public delegate void RemoveCard(int id);
    [Signal] public delegate void Reroll();
    [Signal] public delegate void Battle();
    [Signal] public delegate void Return();
    [Signal] public delegate void ShowTips();
    [Signal] public delegate void HideTips();
    [Signal] public delegate void CardHeld(int id);
    [Signal] public delegate void CardDropped(int id, Vector2 location);

    private Button _rerollButton;
    private Button _battleButton;
    private Button _helpButton;
    private Button _menuButton;
    private Label _fundsLabel;
    private Label _countLabel;
    private Button _helpPanel;
    private int _fundsValue;
    private int _realFundsValue;
    private int _helpMode;

    public override void _Ready()
    {
        base._Ready();

        _fundsLabel = FindNode("FundsLabel") as Label;
        _countLabel = FindNode("CountLabel") as Label;
        _rerollButton = FindNode("RerollButton") as Button;
        _rerollButton.Connect("pressed", this, "OnRerollClicked");
        _battleButton = FindNode("BattleButton") as Button;
        _battleButton.Connect("pressed", this, "OnBattleClicked");
        _helpButton = FindNode("HelpButton") as Button;
        _helpButton.Connect("pressed", this, "OnHelpClicked");
        _menuButton = FindNode("MenuButton") as Button;
        _menuButton.Connect("pressed", this, "OnMenuClicked");
        _helpPanel = FindNode("HelpPanel") as Button;
        _helpPanel.Connect("pressed", this, "OnHelpPanelClicked");
        _helpPanel.Hide();

        Connect("Activate", this, "OnRerollClicked");
    }

    public override void Setup()
    {
        _fundsValue = _realFundsValue = Main.STARTING_FUNDS;
        _fundsLabel.Text = FormatFunds(_fundsValue);
        _countLabel.Text = FormatCount(0);
        EmitSignal("Reroll");
        if (GameStats.Player.FirstTime) ShowHelpMode();
    }

    public override void _Process(float delta)
    {
        if (_fundsValue > _realFundsValue)
            _fundsValue -= 1;
        if (_fundsValue < _realFundsValue)
            _fundsValue += 1;

        _fundsLabel.Text = FormatFunds(_fundsValue);
    }

    public void OnUpdateFunds(int previous, int current, bool check)
    {
        _realFundsValue = current;
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

    public void OnMenuClicked()
    {
        EmitSignal("Return");
    }

    public void OnHelpClicked()
    {
        ShowHelpMode();
    }

    public void ShowHelpMode()
    {
        _helpMode = 0;
        _helpPanel.Show();
        foreach (Node n in _helpPanel.GetChildren())
        {
            if (n is Control c)
                c.Hide();
        }

        if (_helpPanel.GetChildren()[_helpMode] is Label l)
            l.Show();
    }

    public void OnHelpPanelClicked()
    {
        if (_helpPanel.GetChildren()[_helpMode] is Label l)
            l.Hide();

        if (++_helpMode >= _helpPanel.GetChildren().Count)
        {
            _helpPanel.Hide();
            return;
        }

        if (_helpPanel.GetChildren()[_helpMode] is Label l2)
            l2.Show();
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