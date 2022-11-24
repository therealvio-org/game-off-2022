using Godot;
public class Battle : Phase
{
    public enum Outcomes { Loss, Win, Draw }
    [Signal] public delegate void PlayTurn(Turn turn);
    [Signal] public delegate void CredibilityChange(PlayerTypes character, int from, int to);
    [Signal] public delegate void LastTurn();
    [Signal] public delegate void DeclareWinner(Outcomes outcome);
    private BoardState _state;
    private TurnController _turnController;
    public Battle()
    {
        Hand playerHand = HandCache.Get(PlayerTypes.Player);
        //Hand opponentHand = HandCache.Get(PlayerTypes.Opponent);
        Hand opponentHand = Hand.GetRandom();

        _state = new BoardState(new PlayerState(Main.CREDIBILITY), new PlayerState(Main.CREDIBILITY));
        _turnController = new TurnController(playerHand, opponentHand);
    }

    public void ConnectTo(BattleView view)
    {
        view.Connect("NextTurn", this, "OnNextTurn");
        view.Connect("FinishBattle", this, "OnFinishBattle");
        view.Connect("PlayAgain", this, "OnPlayAgain");
        view.Connect("Menu", this, "OnMenu");
        Connect("PlayTurn", view, "OnPlayTurn");
        Connect("LastTurn", view, "OnLastTurn");
        Connect("DeclareWinner", view, "OnDeclareWinner");
    }

    public void OnNextTurn()
    {
        Turn currentTurn = _turnController.Current();
        _state = currentTurn.Perform(_state);

        EmitSignal("PlayTurn", currentTurn);

        if (_turnController.IsLast())
        {
            EmitSignal("LastTurn");
            return;
        }

        _turnController.Advance();
    }

    public void OnFinishBattle()
    {
        EmitSignal("DeclareWinner", _state.GetOutcome());
    }

    public void OnPlayAgain()
    {
        EmitSignal("NextPhase", PhaseTypes.Selection);
    }

    public void OnMenu()
    {
        EmitSignal("NextPhase", PhaseTypes.Menu);
    }
}