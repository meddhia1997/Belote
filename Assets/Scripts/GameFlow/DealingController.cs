using System.Collections;
using UnityEngine;

/// <summary>
/// Spawns a runtime deck anchor and animates dealing packets to each seat.
/// All cards are non-interactable during dealing. After the last packet,
/// hands are re-laid out and (optionally) TurnFlowController is started.
/// </summary>
public class DealingController : MonoBehaviour
{
    [Header("Core")]
    public DeckManager deckManager;

    [Header("Table")]
    public SeatRegistry seatRegistry;
    public AgentFactory agentFactory;

    [Header("UI Roots")]
    [Tooltip("Canvas RectTransform; used as parent for the runtime deck anchor.")]
    public RectTransform canvasRoot;

    [Header("Visuals")]
    public CardView cardViewPrefab;

    [Header("Animation Services")]
    public UIAnimationService uiAnimService;             // low-level (move/select)
    public UIDealingAnimationService dealingAnimService; // high-level (deck->hand)
    public CardAnimSettingsSO cardAnimSettings;          // single-card timings
    public DealingFlowSettingsSO dealingFlowSettings;    // rhythm, packetPattern, deck position

    [Header("Round Autostart")]
    [Tooltip("If true, call TurnFlowController after dealing completes.")]
    public bool autoStartRound = true;
    public TurnFlowController turnFlow;                  // optional; auto-find if not set
    public RoundController roundController;              // optional; round init hook
    public SeatId dealerForThisRound = SeatId.East;      // who dealt (leader = Next)

    // events
    public System.Action OnDealingStarted;
    public System.Action OnDealingCompleted;

    // runtime
    RectTransform _deckAnchorRT;
    Canvas _canvas;

    void Awake()
    {
        // Build registry/agents if needed
        if (seatRegistry != null && agentFactory != null && !seatRegistry.IsBuilt)
            seatRegistry.Build(agentFactory);

        // Canvas
        _canvas = canvasRoot ? canvasRoot.GetComponentInParent<Canvas>() : FindObjectOfType<Canvas>();
        if (!_canvas)
        {
            Debug.LogError("[DealingController] No Canvas found.");
            enabled = false;
            return;
        }

        // Runtime deck anchor
        _deckAnchorRT = new GameObject("DeckAnchor_RT", typeof(RectTransform)).GetComponent<RectTransform>();
        _deckAnchorRT.SetParent(canvasRoot ? canvasRoot : _canvas.transform as RectTransform, false);
        _deckAnchorRT.pivot = _deckAnchorRT.anchorMin = _deckAnchorRT.anchorMax = new Vector2(0.5f, 0.5f);
        _deckAnchorRT.anchoredPosition = dealingFlowSettings ? dealingFlowSettings.deckAnchoredPos : new Vector2(0f, -60f);
        var cg = _deckAnchorRT.gameObject.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;

        // Auto-find TurnFlow if not assigned
        if (!turnFlow) turnFlow = FindObjectOfType<TurnFlowController>(includeInactive: true);
    }

    [ContextMenu("Deal New Round (pattern)")]
    public void DealNewRound()
    {
        StartCoroutine(DealRoutine(dealerForThisRound));
    }

    public IEnumerator DealRoutine(SeatId dealer)
    {
        if (_canvas == null || _deckAnchorRT == null || seatRegistry == null || deckManager == null)
        {
            Debug.LogError("[DealingController] Missing wiring (Canvas/DeckAnchor/SeatRegistry/DeckManager).");
            yield break;
        }

        OnDealingStarted?.Invoke();

        // Reset & shuffle deck
        deckManager.ResetDeck();
        deckManager.ShuffleDeck();

        // 0) LOCK everyone and clear hands
        foreach (var s in seatRegistry.All())
        {
            if (s.Hand == null) continue;
            s.Hand.ClearHand();
            s.Hand.SetInteractable(false); // 🔒 hard lock during dealing
        }

        // 1) Dealing order & pattern
        var order = seatRegistry.OrderAfter(dealer);
        var packets = (dealingFlowSettings && dealingFlowSettings.packetPattern != null && dealingFlowSettings.packetPattern.Length > 0)
                        ? dealingFlowSettings.packetPattern
                        : new int[] { 3, 2, 3 };

        // 2) Deal packets
        for (int p = 0; p < packets.Length; p++)
        {
            int packet = packets[p];
            for (int oi = 0; oi < order.Count; oi++)
            {
                var seat = seatRegistry.Get(order[oi]);
                if (seat == null || seat.Hand == null)
                {
                    Debug.LogError($"[DealingController] Missing seat/hand for {order[oi]}.");
                    yield break;
                }
                yield return StartCoroutine(DealPacketToSeat(seat, packet));
            }

            if (dealingFlowSettings && dealingFlowSettings.interPacketDelay > 0f)
                yield return new WaitForSeconds(dealingFlowSettings.interPacketDelay);
        }

        // 3) Final layout pass (still locked)
        foreach (var s in seatRegistry.All())
            s.Hand?.LayoutFan();

        // 4) Unlock ONLY local seat now (TurnFlow will manage interactivity afterwards)
        foreach (var s in seatRegistry.All())
        {
            bool enable = s.IsLocal;
            s.Hand?.SetInteractable(enable);

            // If LocalHandInput exists, (re)hook relays for new cards
            if (enable)
            {
                var input = s.Hand.GetComponent<LocalHandInput>();
                if (input) input.HookExistingCards();
            }
        }

        OnDealingCompleted?.Invoke();

        // 5) Optionally start the round & notify RoundController
        if (autoStartRound && turnFlow)
        {
            roundController?.BeginNewRound(dealer);  // initialize round score state
            turnFlow.StartRound(dealer);             // leader = Next(dealer)
            yield return StartCoroutine(turnFlow.PlayRound());
        }
    }

    IEnumerator DealPacketToSeat(SeatContext seat, int count)
    {
        var hand = seat.Hand;
        bool faceUpForSeat = seat.IsLocal;

        for (int i = 0; i < count; i++)
        {
            var cardSO = deckManager.DrawCard();
            if (cardSO == null) yield break;

            // 1) Create CardView under Canvas
            var canvasRT = _canvas.transform as RectTransform;
            var cv = Instantiate(cardViewPrefab, canvasRT);
            if (cv == null)
            {
                Debug.LogError("[DealingController] Failed to instantiate CardView prefab.");
                yield break;
            }

            // 2) Visuals (FORCE non-interactive during dealing)
            cv.SetCard(cardSO);
            cv.ShowFace(faceUpForSeat);
            cv.SetInteractable(false); // 🔒

            // 3) Temp add to hand & layout to compute target slot/rotation
            hand.AddCard(cv);
            hand.LayoutFan();

            var handRT = (RectTransform)cv.transform;
            Vector2 targetAnchored = handRT.anchoredPosition;
            Quaternion targetRot   = handRT.localRotation;

            // 4) Animate deck → hand slot via service (single deck origin)
            Vector3 deckWorld = _deckAnchorRT.position;

            if (dealingAnimService != null && uiAnimService != null && cardAnimSettings != null)
            {
                yield return StartCoroutine(
                    dealingAnimService.AnimateCardFromDeckToHand(
                        cv,
                        hand.HandAnchor,
                        deckWorld,
                        targetAnchored,
                        targetRot,
                        _canvas,
                        uiAnimService,
                        cardAnimSettings,
                        dealingFlowSettings
                    )
                );
            }
            else
            {
                // Fallback: snap into place
                uiAnimService?.ReparentToCanvasKeepScreenPos(handRT, canvasRT);
                handRT.SetParent(hand.HandAnchor, false);
                handRT.anchoredPosition = targetAnchored;
                handRT.localRotation = targetRot;
            }

            // (still non-interactable until whole dealing ends)
        }
    }

    // Optional helper if you want to chain rounds: rotate dealer clockwise
    public static SeatId NextDealer(SeatId currentDealer)
    {
        return SeatRegistry.Next(currentDealer);
    }
}
