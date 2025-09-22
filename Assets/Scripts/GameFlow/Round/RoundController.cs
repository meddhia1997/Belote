using System;
using UnityEngine;

public class RoundController : MonoBehaviour, IRoundController
{
    [Header("Policies & Services")]
    public TeamMapSO teamMap;
    public MatchRulesSO matchRules;

    [Header("Rules Source")]
    [Tooltip("If left empty, this will auto-use TurnFlowController.rulesProfile at runtime.")]
    public RulesProfileSO rulesProfileOverride;
    [Tooltip("Optional. If left empty, will auto-FindObjectOfType in Awake().")]
    public TurnFlowController turnFlow;

    [Header("Views")]
    [Tooltip("Drag the ScoreboardView_Text component here (NOT the GameObject). If left empty, will auto-find in children.")]
    public ScoreboardView_Text scoreboardView; // concrete type for reliable serialization

    // runtime
    private IRoundScorer scorer;
    private IScoreboardView scoreboard;
    private int tricksPlayed;

    public event Action<RoundScore> OnRoundFinished;

    // Resolve the active scoring policy from the assigned RulesProfile
    private IScoringPolicy ScoringPolicy
    {
        get
        {
            var profile = rulesProfileOverride != null
                ? rulesProfileOverride
                : (turnFlow != null ? turnFlow.rulesProfile : null);

            return profile != null ? profile.ScoringPolicy : null;
        }
    }

    void Awake()
    {
        scorer = new RoundScorer();

        // Find TurnFlow if not assigned (so we can read its RulesProfile)
        if (turnFlow == null)
            turnFlow = FindObjectOfType<TurnFlowController>(true);

        // Auto-find scoreboard view if not set
        if (scoreboardView == null)
            scoreboardView = GetComponentInChildren<ScoreboardView_Text>(true);

        // Bind interface
        scoreboard = scoreboardView as IScoreboardView;

        if (scoreboard == null)
        {
            Debug.LogError("[RoundController] ScoreboardView not set or does not implement IScoreboardView. " +
                           "Fix: drag the ScoreboardView_Text component (the script) onto RoundController.scoreboardView.");
        }

        if (ScoringPolicy == null)
        {
            Debug.LogError("[RoundController] No ScoringPolicy resolved. " +
                           "Assign RulesProfileSO in RoundController.rulesProfileOverride or on TurnFlowController.rulesProfile.");
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

            // Apply last-trick bonus using the active scoring policy (from RulesProfile)
            var scoring = ScoringPolicy;
            if (scoring != null && rs.lastTrickWinner.HasValue)
            {
                int bonus = scoring.LastTrickBonus;
                if (rs.lastTrickWinner.Value == TeamId.Us) rs.us += bonus;
                else rs.them += bonus;
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
