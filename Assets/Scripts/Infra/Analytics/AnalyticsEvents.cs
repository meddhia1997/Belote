public static class AnalyticsEvents
{
    public static void TrackTrickSafe(IAnalyticsSink sink, int trickIndex, TeamId winnerTeam, int points)
    {
        if (sink == null) return;
        sink.TrackTrick(trickIndex, winnerTeam, points);
    }
}
