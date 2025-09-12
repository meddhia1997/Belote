public interface IAnalyticsSink
{
    void TrackMatchStart(SeatId startingDealer, int targetPoints);
    void TrackMatchStop(int us, int them);
    void TrackNextRound(SeatId nextDealer, int matchUs, int matchThem);
    void TrackRoundEnd(int roundUs, int roundThem, TeamId? lastTrickWinner);
    void TrackMatchEnd(int matchUs, int matchThem);
    void TrackTrick(int trickIndex, TeamId winnerTeam, int points); // optional hook if you call it from TurnFlow
}
