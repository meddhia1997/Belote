public class AnalyticsSink_Null : IAnalyticsSink
{
    public void TrackMatchStart(SeatId startingDealer, int targetPoints) {}
    public void TrackMatchStop(int us, int them) {}
    public void TrackNextRound(SeatId nextDealer, int matchUs, int matchThem) {}
    public void TrackRoundEnd(int roundUs, int roundThem, TeamId? lastTrickWinner) {}
    public void TrackMatchEnd(int matchUs, int matchThem) {}
    public void TrackTrick(int trickIndex, TeamId winnerTeam, int points) {}
}
