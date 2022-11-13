using Godot;
public class Battle : Phase
{
    [Signal] public delegate void PlayCard(int id, PlayerTypes character, Battle battle);
    [Signal] public delegate void CredibilityChange(PlayerTypes character, int from, int to);
    [Signal] public delegate void LastCard();
    [Signal] public delegate void DeclareWinner(PlayerTypes winner);
    private int[] _cards;
    private int _cardIndex;
    private PlayerTypes _turn;
    private BoardState _state;
    public Battle(int[] _playerCards, int[] _opponentCards)
    {
        _cardIndex = 0;
        if (GoesFirst(_playerCards[0], _opponentCards[0]))
            _cards = JoinArrays(_playerCards, _opponentCards);
        else
            _cards = JoinArrays(_opponentCards, _playerCards);

        _state = new BoardState();
    }

    public void ConnectTo(BattleView view)
    {
        view.Connect("NextCard", this, "GetNextCard");
        view.Connect("FinishBattle", this, "OnFinishBattle");
        Connect("PlayCard", view, "OnPlayCard");
        Connect("CredibilityChange", view, "OnCredibilityChange");
        Connect("LastCard", view, "OnLastCard");
        Connect("DeclareWinner", view, "OnDeclareWinner");
    }

    public void GetNextCard()
    {
        EmitSignal("PlayCard", _cards[_cardIndex++], _turn, this);

        if (_cardIndex == _cards.Length)
        {
            EmitSignal("LastCard");
            return;
        }

        ChangeTurn();
    }

    public void OnFinishBattle()
    {
        PlayerTypes winner = _state.GetWinner();
        EmitSignal("DeclareWinner", winner);
    }

    public void ChangeTurn()
    {
        _turn = _turn == PlayerTypes.Player ? PlayerTypes.Opponent : PlayerTypes.Player;
    }

    public bool GoesFirst(int playerCardId, int opponentCardId)
    {
        return true;
    }

    public int[] JoinArrays(int[] first, int[] second)
    {
        int[] collated = new int[first.Length + second.Length];
        for (int i = 0; i < first.Length; i++)
        {
            collated[i * 2] = first[i];
            collated[i * 2 + 1] = second[i];
        }

        return collated;
    }

    public void ModifyCredibility(PlayerTypes owner, int value, PlayerTypes target)
    {
        int from = _state.GetCredibility(target);
        _state.UpdateCredibility(target, value);
        int to = _state.GetCredibility(target);

        EmitSignal("CredibilityChange", target, from, to);
    }
}