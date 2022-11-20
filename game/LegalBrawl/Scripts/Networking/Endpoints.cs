public static class Endpoints
{
    private const string _playerHandExtension = "/prod/v1/playerHand";


    public static string PlayerHand(string playerId)
    {
        return $"{DebugKeys.BASE}{_playerHandExtension}?playerId={playerId}";
    }
}