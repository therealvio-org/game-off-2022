public enum PlayerTypes { Player, Opponent };

public static class PlayerTypeHelper
{
    public static PlayerTypes GetOther(PlayerTypes type)
    {
        if (type == PlayerTypes.Player)
            return PlayerTypes.Opponent;
        return PlayerTypes.Player;
    }
}