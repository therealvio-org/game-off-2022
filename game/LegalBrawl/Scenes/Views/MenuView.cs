using Godot;
using System;

public class MenuView : View
{
    [Signal] public delegate void Play();
    [Signal] public delegate void Tutorial();
    [Signal] public delegate void DataDeleted();
    private Button _playButton;
    private Button _leaderboardButton;
    private Button _helpButton;
    private Button _settingsButton;
    private Popup _playerNamePopup;
    private Popup _settingsPopup;
    private Label _playerName;
    private Label _playerWins;
    private Label _playerLosses;
    private bool _queueSelection = false;
    public override void _Ready()
    {
        base._Ready();

        _playButton = FindNode("PlayButton") as Button;
        _playButton.Connect("pressed", this, "OnPlayClicked");

        _leaderboardButton = FindNode("LeaderboardButton") as Button;
        _leaderboardButton.Disabled = true;

        _helpButton = FindNode("HelpButton") as Button;
        _helpButton.Connect("pressed", this, "OnHelpClicked");

        _settingsButton = FindNode("SettingsButton") as Button;
        _settingsButton.Connect("pressed", this, "OnSettingsClicked");

        _playerNamePopup = FindNode("PlayerNamePopup") as Popup;
        _playerNamePopup.Connect("Finished", this, "UpdatePlayerName");

        _settingsPopup = FindNode("SettingsPopup") as Popup;

        _playerName = FindNode("PlayerName") as Label;
        _playerWins = FindNode("PlayerWins") as Label;
        _playerLosses = FindNode("PlayerLosses") as Label;

        Connect("DataDeleted", this, "OnDataDeleted");
    }

    public override void Setup()
    {
        if (GameStats.HasPlayerName())
        {
            _playerName.Text = GameStats.Player.PlayerName;
            _playerWins.Text = $"{GameStats.Player.Wins} - Wins";
            _playerLosses.Text = $"{GameStats.Player.Losses} - Losses";
        }
        else
        {
            _playerName.Text = "";
            _playerWins.Text = "";
            _playerLosses.Text = "";
        }
    }

    public void OnPlayClicked()
    {
        EmitSignal("Play");
    }

    public void OnHelpClicked()
    {
        EmitSignal("Tutorial");
    }

    public void OnSettingsClicked()
    {
        _settingsPopup.Popup_();
    }

    public void OnGetPlayerName(bool queueSelection = false)
    {
        _queueSelection = queueSelection;
        _playerNamePopup.Popup_();
    }

    public void OnDataDeleted()
    {
        _playerName.Text = "";
        _playerWins.Text = "";
        _playerLosses.Text = "";
    }

    public void UpdatePlayerName(string name)
    {
        GameStats.SetPlayerName(name);
        _playerNamePopup.Hide();
        if (_queueSelection)
            EmitSignal("Play");
    }
}