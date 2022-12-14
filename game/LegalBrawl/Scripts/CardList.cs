using System.Collections.Generic;

public class CardList
{
    private List<Card> _cards;
    public Card LastHeld;
    public bool IsHeld { get => LastHeld != null && LastHeld.IsHeld; }
    public Card LastHovered;
    public bool IsHovered { get => LastHovered != null && LastHovered.IsHovered; }

    public void MoveHeldToHover()
    {
        int from = _cards.IndexOf(LastHeld);
        int to = _cards.IndexOf(LastHovered);
        if (from < to) to++;

        _cards[from] = null;
        _cards.Insert(to, LastHeld);
        _cards.Remove(null);
    }

    public CardList()
    {
        _cards = new List<Card>();
    }

    public void Add(Card card)
    {
        card.List = this;
        _cards.Add(card);
    }
    public void RemoveId(int id)
    {
        Card card = _cards.FindLast((Card c) => c.Display.Resource.Id == id);
        if (card == null)
            throw new System.Exception("Trying to remove a card that isn't present");

        if (LastHeld == card)
            LastHeld = null;

        if (LastHovered == card)
            LastHovered = null;

        _cards.Remove(card);
        card.QueueFree();
    }

    public void RemoveAt(int index)
    {
        _cards[index].List = null;
        _cards.RemoveAt(index);
    }

    public Card Get(int index) => _cards[index];
    public int Count => _cards.Count;
    public List<Card> List => _cards;

    public void RemoveAll()
    {
        foreach (Card c in _cards)
        {
            c.List = null;
            c.QueueFree();
        }
        _cards.RemoveAll((Card c) => true);
    }
}