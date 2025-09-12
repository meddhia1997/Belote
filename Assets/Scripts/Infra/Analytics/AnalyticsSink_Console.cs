using UnityEngine;

public class AnalyticsSink_Console : MonoBehaviour, IAnalyticsSink
{
    public void TrackMatchStart(SeatId startingDealer, int targetPoints)
        => Debug.Log($"[ANALYTICS] MatchStart dealer={startingDealer} target={targetPoints}");

    public void TrackMatchStop(int us, int them)
        => Debug.Log($"[ANALYTICS] MatchStop us={us} them={them}");

    public void TrackNextRound(SeatId nextDealer, int matchUs, int matchThem)
        => Debug.Log($"[ANALYTICS] NextRound dealer={nextDealer} matchUs={matchUs} matchThem={matchThem}");

    public void TrackRoundEnd(int roundUs, int roundThem, TeamId? lastTrickWinner)
        => Debug.Log($"[ANALYTICS] RoundEnd us={roundUs} them={roundThem} last={lastTrickWinner}");

    public void TrackMatchEnd(int matchUs, int matchThem)
        => Debug.Log($"[ANALYTICS] MatchEnd us={matchUs} them={matchThem}");

    public void TrackTrick(int trickIndex, TeamId winnerTeam, int points)
        => Debug.Log($"[ANALYTICS] Trick idx={trickIndex} winnerTeam={winnerTeam} pts={points}");
}
