public static class MatchResults
{
    // Temporarily stores results so the end-game scene can display them.
    public static string WinnerName { get; private set; }
    public static float PlayerOneDamageDealt { get; private set; }
    public static float PlayerTwoDamageDealt { get; private set; }
    public static bool HasResults { get; private set; }

    public static void SetResults(
        string winnerName,
        float playerOneDamageDealt,
        float playerTwoDamageDealt)
    {
        // Save the winner and both players' damage totals.
        WinnerName = winnerName;
        PlayerOneDamageDealt = playerOneDamageDealt;
        PlayerTwoDamageDealt = playerTwoDamageDealt;
        HasResults = true;
    }

    public static void Clear()
    {
        // Remove results before starting another match.
        WinnerName = string.Empty;
        PlayerOneDamageDealt = 0f;
        PlayerTwoDamageDealt = 0f;
        HasResults = false;
    }
}
