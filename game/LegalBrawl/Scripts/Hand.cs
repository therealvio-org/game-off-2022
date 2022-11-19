using System;

public class Hand
{
    private int[] _cards;
    public Hand(int[] cards)
    {
        _cards = cards;
    }

    public static Hand GetRandom()
    {
        int[] cards = new int[7];

        for (int i = 0; i < 7; i++)
        {
            cards[i] = CardLibrary.DrawRandomId();
        }

        return new Hand(cards);
    }

    public BaseCard[] GetCards()
    {
        BaseCard[] cards = new BaseCard[_cards.Length];
        int index = 0;
        foreach (int cid in _cards)
        {
            cards[index++] = CardLibrary.Get(cid);
        }

        return cards;
    }

    public int[] GetIds()
    {
        return _cards;
    }

    public int TotalCost()
    {
        int total = 0;
        foreach (int cid in _cards)
            total += CardLibrary.Get(cid).Cost;

        return total;
    }

    public bool Validate()
    {
        return TotalCost() <= Main.STARTING_FUNDS && _cards.Length == Main.MAX_HAND;
    }

    public int Size() => _cards.Length;

    public BaseCard FirstCard() => CardLibrary.Get(_cards[0]);
}