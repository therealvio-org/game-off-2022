using Godot;
using System.Collections.Generic;

public class CardLibrary : Node
{
    private static CardLibrary _instance;
    private Dictionary<int, BaseCard> _database;
    private static RandomNumberGenerator _random;
    public override void _Ready()
    {
        _instance = this;
        _random = new RandomNumberGenerator();
        ReadDeck((DeckResource)GD.Load("res://Resources/MainDeck.tres"));
    }

    public static BaseCard Get(int id)
    {
        return _instance._database[id];
    }

    private void ReadDeck(DeckResource deck)
    {
        _database = new Dictionary<int, BaseCard>();

        foreach (BaseCard card in deck.Cards)
        {
            _database.Add(card.Id, card);
            GD.Print(card.Name);
        }
    }

    public static int DrawRandomId()
    {
        int[] keys = new int[_instance._database.Count];
        _instance._database.Keys.CopyTo(keys, 0);
        return keys[_random.RandiRange(0, keys.Length - 1)];
    }
}
