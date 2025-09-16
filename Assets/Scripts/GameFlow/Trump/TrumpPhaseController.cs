using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Runs immediately after dealing (before first trick).
/// Chooses trump via ITrumpSource (SO) or via UI and raises OnTrumpDecided(Suit).
/// NOTE: This controller DOES NOT start the round; it only decides the trump
/// and notifies listeners (e.g., DealingController) to continue the flow.
/// </summary>
public class TrumpPhaseController : MonoBehaviour
{
    [Header("Policy (SO)")]
    [Tooltip("ScriptableObject that implements ITrumpSource (Random, Preset, later Bidding).")]
    public ScriptableObject trumpSourceAsset; // must implement ITrumpSource

    [Header("Views")]
    public TrumpBannerView banner;            // optional visual feedback

    [Header("Behavior")]
    [Tooltip("If true and the ITrumpSource can decide immediately, we auto-decide on Begin().")]
    public bool autoDecideIfSourceIsImmediate = true;

    [Tooltip("Optional 'thinking' delay (seconds) before auto-decide is emitted.")]
    public float autoDecideDelay = 0.4f;

    // Event: listeners (e.g., DealingController) subscribe to continue flow
    public event Action<Suit> OnTrumpDecided;

    // runtime
    private ITrumpSource _source;
    private bool _phaseActive;
    private SeatId _dealerOfThisPhase = SeatId.South;

    void Awake()
    {
        _source = trumpSourceAsset as ITrumpSource;
        if (_source == null)
            Debug.LogError("[TrumpPhaseController] trumpSourceAsset must implement ITrumpSource.");
    }

    /// <summary>
    /// Entry point called by DealingController once cards are dealt.
    /// If the source is immediate and autoDecide is true, we auto-decide after a small delay.
    /// Otherwise, show the banner and wait for DecideTrump(...) (e.g., from UI).
    /// </summary>
    public void Begin(SeatId dealer)
    {
        if (_source == null)
        {
            Debug.LogError("[TrumpPhaseController] No ITrumpSource assigned.");
            return;
        }

        _dealerOfThisPhase = dealer;
        _phaseActive = true;
        Debug.Log($"[TrumpPhase] Begin (dealer={dealer})");

        // You can open a UI here (buttons to choose suit), or auto-decide if policy is immediate.
        if (banner) banner.Show(Suit.None, dealer); // show placeholder (no suit yet)

        if (autoDecideIfSourceIsImmediate && _source.IsImmediate)
        {
            // e.g., Random/Preset policies return instantly
            StartCoroutine(AutoDecideRoutine());
        }
        else
        {
            // UI path: wait for player's input to call DecideTrump(...)
            // If you want an AI/bidding flow, call DecideTrump from elsewhere when done.
        }
    }

    /// <summary>
    /// Public entry for UI/AI to finalize trump selection.
    /// This will raise OnTrumpDecided exactly once per phase.
    /// </summary>
    public void DecideTrump(Suit chosen)
    {
        if (!_phaseActive) return;

        _phaseActive = false;

        if (banner) banner.Show(chosen, _dealerOfThisPhase);

        Debug.Log($"[TrumpPhase] Trump decided = {chosen}");
        OnTrumpDecided?.Invoke(chosen);
    }

    /// <summary>
    /// Optional cancellation (no trump chosen). Does NOT invoke event.
    /// </summary>
    public void CancelPhase()
    {
        if (!_phaseActive) return;
        _phaseActive = false;
        Debug.LogWarning("[TrumpPhase] Phase cancelled without a trump selection.");
        // keep banner as-is or hide it if you want:
        // if (banner) banner.Hide();
    }

    // --- helpers ---
    private IEnumerator AutoDecideRoutine()
    {
        if (autoDecideDelay > 0f)
            yield return new WaitForSeconds(autoDecideDelay);

        // Query policy for immediate trump
        var trump = _source.DecideTrump(_dealerOfThisPhase);
        DecideTrump(trump);
    }
}
