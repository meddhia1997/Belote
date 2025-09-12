using System;
using UnityEngine;

public class MatchController : MonoBehaviour, IMatchController
{
    [Header("Refs")]
    public RoundController roundController;
    public DealingController dealingController;

    [Header("Rules/Policies")]
    public MatchRulesSO matchRules;
    public RotatingDealerPolicySO dealerPolicyAsset;

    [Header("Views")]
    public ScoreboardView_Text scoreboardView;

    [Header("Telemetry")]
    public MonoBehaviour analyticsSinkSource;

    // runtime
    private IDealerPolicy dealerPolicy;
    private IScoreboardView scoreboard;
    private IAnalyticsSink analytics;
    private MatchScore matchScore;
    private SeatId currentDealer;
    private bool matchRunning;

    public event Action<MatchScore> OnMatchFinished;

    void Awake()
    {
        if (!roundController)   Debug.LogError("[MatchController] RoundController not assigned.");
        if (!dealingController) Debug.LogError("[MatchController] DealingController not assigned.");
        if (!matchRules)        Debug.LogError("[MatchController] MatchRulesSO not assigned.");

        dealerPolicy = dealerPolicyAsset;
        if (dealerPolicy == null)
            Debug.LogError("[MatchController] dealerPolicyAsset missing.");

        scoreboard = scoreboardView as IScoreboardView;
        if (scoreboard == null)
            Debug.LogError("[MatchController] scoreboardView must implement IScoreboardView.");

        analytics = analyticsSinkSource as IAnalyticsSink ?? new AnalyticsSink_Null();
    }

    void OnEnable()
    {
        if (roundController != null)
            roundController.OnRoundFinished += HandleRoundFinished;
    }

    void OnDisable()
    {
        if (roundController != null)
            roundController.OnRoundFinished -= HandleRoundFinished;
    }

    // ------------------ PUBLIC API ------------------

    public void StartMatch(SeatId startingDealer)
    {
        matchRunning = true;
        currentDealer = startingDealer;

        matchScore = new MatchScore
        {
            us = 0,
            them = 0,
            target = matchRules != null ? matchRules.TargetPoints : 1000
        };

        scoreboard?.SetMatch(matchScore.us, matchScore.them);
        analytics.TrackMatchStart(startingDealer, matchScore.target);

        BeginRound(currentDealer);
    }

    public void StopMatch()
    {
        matchRunning = false;
        analytics.TrackMatchStop(matchScore.us, matchScore.them);
    }

    public MatchScore GetCurrentMatchScore() => matchScore;

    /// <summary>
    /// 🔹 Context function: one-call entry point to start a match
    /// Use this from a bootstrapper or button.
    /// </summary>
    [ContextMenu("Start Match (South Dealer)")]
    public void StartMatchContext()
    {
        // Example: always start with South as dealer (customize as you like)
        StartMatch(SeatId.South);
    }

    // ------------------ INTERNAL ------------------

    void BeginRound(SeatId dealer)
    {
        if (!matchRunning) return;

        if (dealingController != null)
        {
            dealingController.dealerForThisRound = dealer;
            dealingController.DealNewRound();
        }
        else
        {
            Debug.LogError("[MatchController] DealingController is null; cannot begin round.");
        }
    }

    void HandleRoundFinished(RoundScore round)
    {
        if (!matchRunning) return;

        matchScore.us   += round.us;
        matchScore.them += round.them;
        scoreboard?.SetMatch(matchScore.us, matchScore.them);

        analytics.TrackRoundEnd(round.us, round.them, round.lastTrickWinner);

        if (IsMatchOver(matchScore, matchRules))
        {
            matchRunning = false;
            analytics.TrackMatchEnd(matchScore.us, matchScore.them);
            OnMatchFinished?.Invoke(matchScore);
            return;
        }

        currentDealer = dealerPolicy != null ? dealerPolicy.NextDealer(currentDealer)
                                             : SeatRegistry.Next(currentDealer);

        analytics.TrackNextRound(currentDealer, matchScore.us, matchScore.them);
        BeginRound(currentDealer);
    }

    bool IsMatchOver(MatchScore score, IMatchRules rules)
    {
        if (rules == null) return score.us >= 1000 || score.them >= 1000;

        bool usWin   = score.us   >= rules.TargetPoints;
        bool themWin = score.them >= rules.TargetPoints;

        if (!rules.WinByTwo) return usWin || themWin;

        if (usWin   && score.us   - score.them >= 2) return true;
        if (themWin && score.them - score.us   >= 2) return true;
        return false;
    }
}
