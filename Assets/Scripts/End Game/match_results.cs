public static class MatchResults
{
    public static string WinnerName { get; private set; }
    public static float PlayerOneDamageDealt { get; private set; }
    public static float PlayerTwoDamageDealt { get; private set; }
    public static bool HasResults { get; private set; }

    public static void SetResults(
        string winnerName,
        float playerOneDamageDealt,
        float playerTwoDamageDealt)
    {
        WinnerName = winnerName;
        PlayerOneDamageDealt = playerOneDamageDealt;
        PlayerTwoDamageDealt = playerTwoDamageDealt;
        HasResults = true;
    }

    public static void Clear()
    {
        WinnerName = string.Empty;
        PlayerOneDamageDealt = 0f;
        PlayerTwoDamageDealt = 0f;
        HasResults = false;
    }
}
