using Godot;
public class Battle : Phase
{
    [Signal] public delegate void PlayCard(int id, Lawyer.Character character);
    private int[] _cards;
    private int _cardIndex;
    private Lawyer.Character _turn;
    public Battle(int[] _playerCards, int[] _opponentCards)
    {
        _cardIndex = 0;
        if (GoesFirst(_playerCards[0], _opponentCards[0]))
            _cards = JoinArrays(_playerCards, _opponentCards);
        else
            _cards = JoinArrays(_opponentCards, _playerCards);
    }

    public void ConnectTo(BattleView view)
    {
        view.Connect("NextCard", this, "GetNextCard");
        Connect("PlayCard", view, "OnPlayCard");
    }

    public void GetNextCard()
    {
        EmitSignal("PlayCard", _cards[_cardIndex++], _turn);
        ChangeTurn();
    }

    public void ChangeTurn()
    {
        _turn = _turn == Lawyer.Character.Player ? Lawyer.Character.Opponent : Lawyer.Character.Player;
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
}