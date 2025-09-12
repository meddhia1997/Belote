using System;
using UnityEngine;

public class RoundController : MonoBehaviour, IRoundController
{
    [Header("Policies & Services")]
    public TeamMapSO teamMap;
    public ClassicScorePolicySO scorePolicy;
    public MatchRulesSO matchRules;

    [Header("Views")]
    [Tooltip("Drag the ScoreboardView_Text component here (NOT the GameObject). If left empty, will auto-find in children.")]
    public ScoreboardView_Text scoreboardView; // concrete type for reliable serialization

    // runtime
    private IRoundScorer scorer;
    private IScoreboardView scoreboard;
    private int tricksPlayed;

    public event Action<RoundScore> OnRoundFinished;

    void Awake()
    {
        scorer = new RoundScorer();

        // Auto-find if not assigned
        if (scoreboardView == null)
            scoreboardView = GetComponentInChildren<ScoreboardView_Text>(true);

        // Bind interface
        scoreboard = scoreboardView as IScoreboardView;

        if (scoreboard == null)
        {
            Debug.LogError("[RoundController] ScoreboardView not set or does not implement IScoreboardView. " +
                           "Fix: drag the ScoreboardView_Text component (the script) onto RoundController.scoreboardView.");
        }
    }

    public void BeginNewRound(SeatId dealer)
    {
        scorer.ResetRound();
        tricksPlayed = 0;
        scoreboard?.SetRound(0, 0);
    }

    public void OnTrickResolved(SeatId winnerSeat, int trickPoints)
    {
        var team = teamMap.GetTeam(winnerSeat);
        scorer.AddTrick(team, trickPoints);
        tricksPlayed++;

        if (tricksPlayed == 8)
        {
            scorer.SetLastTrickWinner(team);
            var rs = scorer.GetRoundScore();

            if (rs.lastTrickWinner.HasValue)
            {
                if (rs.lastTrickWinner.Value == TeamId.Us) rs.us += scorePolicy.LastTrickBonus();
                else rs.them += scorePolicy.LastTrickBonus();
            }

            scoreboard?.SetRound(rs.us, rs.them);
            scoreboard?.FlashWinner(rs.us > rs.them ? TeamId.Us : TeamId.Them);

            OnRoundFinished?.Invoke(rs);
        }
        else
        {
            var rs = scorer.GetRoundScore();
            scoreboard?.SetRound(rs.us, rs.them);
        }
    }
}
