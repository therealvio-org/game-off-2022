using Godot;

public class HandCache : Node
{
    private static HandCache _instance;
    private Hand _playerHand = null;
    private Hand _opponentHand = null;

    public override void _Ready()
    {
        _instance = this;
    }

    public static void Store(Hand hand, PlayerTypes who = PlayerTypes.Player)
    {
        switch (who)
        {
            case PlayerTypes.Player: _instance._playerHand = hand; return;
            case PlayerTypes.Opponent: _instance._opponentHand = hand; return;
        }
    }

    public static bool Has(PlayerTypes who)
    {
        switch (who)
        {
            case PlayerTypes.Player: return _instance._playerHand == null;
            case PlayerTypes.Opponent: return _instance._opponentHand == null;
        }
        return false;
    }

    public static Hand Get(PlayerTypes who)
    {
        switch (who)
        {
            case PlayerTypes.Player: return _instance._playerHand;
            case PlayerTypes.Opponent: return _instance._opponentHand;
        }
        throw new System.Exception("No player");
    }

    public static void Remove(PlayerTypes who)
    {
        Store(null, who);
    }
}