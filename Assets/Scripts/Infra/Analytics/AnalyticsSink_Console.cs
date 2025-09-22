using UnityEngine;

/// <summary>
/// Console analytics sink with rich, stateful logging:
/// - Logs active RulesProfile + policies (types) at first use
/// - Tracks running round totals from Trick events (us/them)
/// - Validates 162-point invariant at RoundEnd (cards 152 + last trick 10)
/// - Logs trump/contract mode (Sun vs Hokom) inferred from RulesProfile.TrumpPolicy.CurrentTrump
/// - Emits compact snapshots on every callback so you can paste the Console log and I can audit quickly
/// 
/// NOTE: Works with existing IAnalyticsSink; no interface changes required.
/// It opportunistically inspects TurnFlowController and RulesProfileSO at runtime.
/// </summary>
public class AnalyticsSink_Console : MonoBehaviour, IAnalyticsSink
{
    [Header("Verbosity")]
    [Tooltip("If true, logs additional details like active policy type names and invariant checks.")]
    public bool verbose = true;

    [Tooltip("If true, prints a compact state snapshot (trump, trickIndex, leader) on each event when available.")]
    public bool snapshotEachEvent = true;

    [Header("Scene Wiring (optional)")]
    [Tooltip("If left empty, this sink will auto-Find at first use.")]
    public TurnFlowController turnFlow;
    [Tooltip("Optional manual override. If null, will read from turnFlow.rulesProfile.")]
    public RulesProfileSO rulesProfileOverride;

    // === Internal running state (per round) ===
    private int roundUs;
    private int roundThem;
    private bool roundActive;
    private bool printedProfileOnce;

    // === Cached refs resolved lazily ===
    private RulesProfileSO _activeProfile;

    // -------------- Interface methods --------------

    public void TrackMatchStart(SeatId startingDealer, int targetPoints)
    {
        EnsureRefs();
        if (!_activeProfile)
        {
            Debug.LogWarning("[ANALYTICS] MatchStart but no active RulesProfile could be resolved.");
        }

        LogHeader("MatchStart");
        Debug.Log($"[ANALYTICS] MatchStart dealer={startingDealer} target={targetPoints}");
        if (verbose) PrintProfileOnce();
        if (snapshotEachEvent) PrintSnapshot("on MatchStart");
    }

    public void TrackMatchStop(int us, int them)
    {
        LogHeader("MatchStop");
        Debug.Log($"[ANALYTICS] MatchStop us={us} them={them}");
        if (snapshotEachEvent) PrintSnapshot("on MatchStop");
    }

    public void TrackNextRound(SeatId nextDealer, int matchUs, int matchThem)
    {
        EnsureRefs();
        roundUs = 0;
        roundThem = 0;
        roundActive = true;

        LogHeader("NextRound");
        Debug.Log($"[ANALYTICS] NextRound dealer={nextDealer} matchUs={matchUs} matchThem={matchThem}");

        // Contract mode (Sun vs Hokom) from trump
        var trump = GetTrump(out var trumpReadable);
        if (verbose)
        {
            string contract = trump == Suit.None ? "SUN (no-trump)" : $"HOKOM (trump={trumpReadable})";
            Debug.Log($"[ANALYTICS] Round mode = {contract}");
        }

        if (snapshotEachEvent) PrintSnapshot("on NextRound");
    }

    public void TrackRoundEnd(int roundUsOfficial, int roundThemOfficial, TeamId? lastTrickWinner)
    {
        LogHeader("RoundEnd");

        // Invariant check: our running sum + last trick bonus should match officials
        int lastBonus = GetLastTrickBonusSafe();
        int usCalc = roundUs + (lastTrickWinner == TeamId.Us   ? lastBonus : 0);
        int thCalc = roundThem + (lastTrickWinner == TeamId.Them ? lastBonus : 0);

        Debug.Log($"[ANALYTICS] RoundEnd us={roundUsOfficial} them={roundThemOfficial} last={lastTrickWinner}");

        if (verbose)
        {
            Debug.Log($"[ANALYTICS] RoundCalc (running+bonus): usCalc={usCalc} themCalc={thCalc} (running: us={roundUs} them={roundThem} bonus={lastBonus})");

            int totalOfficial = roundUsOfficial + roundThemOfficial;
            int totalCalc     = usCalc + thCalc;

            // Belote/Baloot classic invariant: 162 points per round (152 from cards + 10 last trick)
            bool officialOk = (totalOfficial == 162);
            bool calcOk     = (totalCalc == 162);

            Debug.Log($"[ANALYTICS] Invariant totals: official={totalOfficial} {(officialOk ? "OK" : "FAIL")} | calc={totalCalc} {(calcOk ? "OK" : "FAIL")}");

            if (!officialOk || !calcOk)
            {
                Debug.LogWarning("[ANALYTICS] ⚠️ Round total mismatch detected. Save this log; we can diff resolver/scoring.");
            }
        }

        roundActive = false;
        if (snapshotEachEvent) PrintSnapshot("on RoundEnd");
    }

    public void TrackMatchEnd(int matchUs, int matchThem)
    {
        LogHeader("MatchEnd");
        Debug.Log($"[ANALYTICS] MatchEnd us={matchUs} them={matchThem}");
        if (snapshotEachEvent) PrintSnapshot("on MatchEnd");
    }

    public void TrackTrick(int trickIndex, TeamId winnerTeam, int points)
    {
        // Maintain our running totals for the round
        if (roundActive)
        {
            if (winnerTeam == TeamId.Us) roundUs += points;
            else if (winnerTeam == TeamId.Them) roundThem += points;
        }

        LogHeader("Trick");
        Debug.Log($"[ANALYTICS] Trick idx={trickIndex} winnerTeam={winnerTeam} pts={points}");

        if (verbose)
        {
            var trump = GetTrump(out var trumpReadable);
            string contract = trump == Suit.None ? "SUN" : $"HOKOM({trumpReadable})";
            Debug.Log($"[ANALYTICS] Trick context: contract={contract} running(us={roundUs}, them={roundThem})");
        }

        if (snapshotEachEvent) PrintSnapshot($"after Trick {trickIndex}");
    }

    // -------------- Helpers --------------

    void EnsureRefs()
    {
        if (!turnFlow)
            turnFlow = FindObjectOfType<TurnFlowController>(true);

        if (!_activeProfile)
            _activeProfile = rulesProfileOverride ? rulesProfileOverride : (turnFlow ? turnFlow.rulesProfile : null);
    }

    Suit GetTrump(out string readable)
    {
        EnsureRefs();
        var trump = Suit.None;

        if (turnFlow != null)
            trump = turnFlow.CurrentTrump;
        else if (_activeProfile != null && _activeProfile.TrumpPolicy != null)
            trump = _activeProfile.TrumpPolicy.CurrentTrump;

        readable = trump.ToString();
        return trump;
    }

    int GetLastTrickBonusSafe()
    {
        EnsureRefs();
        try
        {
            var sp = _activeProfile != null ? _activeProfile.ScoringPolicy : null;
            return sp != null ? sp.LastTrickBonus : 10; // sensible default
        }
        catch { return 10; }
    }

    void PrintProfileOnce()
    {
        if (printedProfileOnce) return;
        printedProfileOnce = true;

        if (_activeProfile == null)
        {
            Debug.LogWarning("[ANALYTICS] No RulesProfile available to print.");
            return;
        }

        string profileName = _activeProfile.name;
        string trumpT = _activeProfile.TrumpPolicy   != null ? _activeProfile.TrumpPolicy.GetType().Name   : "null";
        string orderT = _activeProfile.OrderingPolicy!= null ? _activeProfile.OrderingPolicy.GetType().Name: "null";
        string scoreT = _activeProfile.ScoringPolicy != null ? _activeProfile.ScoringPolicy.GetType().Name : "null";
        string legalT = _activeProfile.LegalMovePolicy!= null? _activeProfile.LegalMovePolicy.GetType().Name: "null";
        string trickT = _activeProfile.TrickResolver  != null ? _activeProfile.TrickResolver.GetType().Name : "null";

        Debug.Log($"[ANALYTICS] ActiveProfile='{profileName}' policies: Trump={trumpT}, Ordering={orderT}, Scoring={scoreT}, Legal={legalT}, TrickResolver={trickT}");
    }

    void PrintSnapshot(string cause)
    {
        EnsureRefs();
        if (turnFlow == null)
        {
            Debug.Log($"[ANALYTICS] Snapshot {cause}: no TurnFlowController found.");
            return;
        }

        string trumpReadable;
        var trump = GetTrump(out trumpReadable);

        // Try to peek minimal trick/turn info
        int tIdx = -1;
        SeatId leader = SeatId.North;
        Suit leadSuit = Suit.None;

        try
        {
            var ctxField = typeof(TurnFlowController).GetField("_ctx", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var ctx = ctxField != null ? ctxField.GetValue(turnFlow) : null;
            if (ctx != null)
            {
                var trickProp = ctx.GetType().GetProperty("CurrentTrick");
                var trick = trickProp != null ? trickProp.GetValue(ctx) : null;

                var idxProp = ctx.GetType().GetProperty("TrickIndex");
                tIdx = idxProp != null ? (int)idxProp.GetValue(ctx) : -1;

                if (trick != null)
                {
                    var leadSuitField = trick.GetType().GetField("leadSuit");
                    if (leadSuitField != null) leadSuit = (Suit)leadSuitField.GetValue(trick);

                    var leaderField = trick.GetType().GetField("leader");
                    if (leaderField != null) leader = (SeatId)leaderField.GetValue(trick);
                }
            }
        }
        catch
        {
            // Reflection is best-effort; keep it silent if not available.
        }

        Debug.Log($"[ANALYTICS] Snapshot {cause}: trump={trumpReadable} trickIndex={tIdx} leader={leader} leadSuit={leadSuit} running(us={roundUs}, them={roundThem})");
    }

    static void LogHeader(string tag)
    {
        Debug.Log($"[ANALYTICS] ---------- {tag} ----------");
    }
}
