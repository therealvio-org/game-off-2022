using Godot;
public class Battle : Phase
{
    [Signal] public delegate void PlayCard(int id, PlayerTypes character, Battle battle);
    [Signal] public delegate void CredibilityChange(PlayerTypes character, int from, int to);
    [Signal] public delegate void LastCard();
    [Signal] public delegate void DeclareWinner(PlayerTypes winner);
    private BoardState _state;
    private TurnController _turnController;
    public Battle()
    {
        Hand playerHand = HandCache.Get(PlayerTypes.Player);
        //Hand opponentHand = HandCache.Get(PlayerTypes.Opponent);
        Hand opponentHand = Hand.GetRandom();

        _state = new BoardState();
        _turnController = new TurnController(playerHand, opponentHand);
    }

    public void ConnectTo(BattleView view)
    {
        view.Connect("NextCard", this, "GetNextCard");
        view.Connect("FinishBattle", this, "OnFinishBattle");
        view.Connect("PlayAgain", this, "OnPlayAgain");
        Connect("PlayCard", view, "OnPlayCard");
        Connect("CredibilityChange", view, "OnCredibilityChange");
        Connect("LastCard", view, "OnLastCard");
        Connect("DeclareWinner", view, "OnDeclareWinner");
    }

    public void GetNextCard()
    {
        Turn currentTurn = _turnController.Current();
        EmitSignal("PlayCard", currentTurn.Id, currentTurn.Owner, this);

        if (_turnController.IsLast())
        {
            EmitSignal("LastCard");
            return;
        }

        _turnController.Advance();
    }

    public void OnFinishBattle()
    {
        PlayerTypes winner = _state.GetWinner();
        EmitSignal("DeclareWinner", winner);
    }

    public void OnPlayAgain()
    {
        EmitSignal("NextPhase", PhaseTypes.Selection);
    }

    public void ModifyCredibility(PlayerTypes owner, int value, PlayerTypes target)
    {
        int from = _state.GetCredibility(target);
        _state.UpdateCredibility(target, value);
        int to = _state.GetCredibility(target);

        EmitSignal("CredibilityChange", target, from, to);
    }
}