using Godot;
using System.Collections.Generic;

public class CardLibrary : Node
{
    private CardLibrary _instance;
    private Dictionary<int, BaseCard> _cardDatabase;
    public override void _Ready()
    {
        _instance = this;
    }

    public BaseCard Get(int id)
    {
        return _cardDatabase[id];
    }
}
