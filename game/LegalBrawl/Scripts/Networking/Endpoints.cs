public static class Endpoints
{
    private const string _playerHandExtension = "/prod/v1/playerHand";


    public static string PlayerHand(string query = "")
    {
        return $"{DebugKeys.BASE}{_playerHandExtension}{query}";
    }

    public static string PlayerIdQuery(string playerId)
    {
        return $"?playerId={playerId}";
    }
}