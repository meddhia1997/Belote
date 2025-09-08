using System.Collections;
using UnityEngine;

public class DealingController : MonoBehaviour
{
    [Header("Core")]
    public DeckManager deckManager;

    [Header("Table")]
    public SeatRegistry seatRegistry;
    public AgentFactory agentFactory;

    [Header("UI Roots")]
    [Tooltip("Canvas RectTransform; used to place the runtime deck anchor.")]
    public RectTransform canvasRoot;

    [Header("Visuals")]
    public CardView cardViewPrefab;

    [Header("Anim Services")]
    public UIAnimationService uiAnimService;             // low-level (select/move)
    public UIDealingAnimationService dealingAnimService; // high-level (deck->hand)
    public CardAnimSettingsSO cardAnimSettings;          // per-card anim
    public DealingFlowSettingsSO dealingFlowSettings;    // rhythm, pattern, deck pos

    // runtime
    RectTransform _deckAnchorRT;
    Canvas _canvas;

    void Awake()
    {
        seatRegistry.Build(agentFactory);

        _canvas = canvasRoot ? canvasRoot.GetComponentInParent<Canvas>() : FindObjectOfType<Canvas>();
        if (!_canvas) { Debug.LogError("[DealingController] No Canvas found."); return; }

        // Spawn the runtime deck anchor (no scene object needed)
        _deckAnchorRT = new GameObject("DeckAnchor_RT", typeof(RectTransform)).GetComponent<RectTransform>();
        _deckAnchorRT.SetParent(canvasRoot ? canvasRoot : _canvas.transform as RectTransform, false);
        _deckAnchorRT.pivot = _deckAnchorRT.anchorMin = _deckAnchorRT.anchorMax = new Vector2(0.5f, 0.5f);
        _deckAnchorRT.anchoredPosition = dealingFlowSettings ? dealingFlowSettings.deckAnchoredPos : new Vector2(0, -60f);

        var cg = _deckAnchorRT.gameObject.AddComponent<CanvasGroup>(); cg.blocksRaycasts = false;
    }

    [ContextMenu("Deal New Round (3-2-3)")]
    public void DealNewRound()
    {
        StartCoroutine(DealRoutine(SeatId.East)); // example dealer
    }

    public IEnumerator DealRoutine(SeatId dealer)
    {
        if (!_canvas || !_deckAnchorRT) yield break;

        deckManager.ResetDeck();
        deckManager.ShuffleDeck();

        // 0) Hard-lock ALL seats during dealing
        foreach (var seat in seatRegistry.All())
        {
            seat.Hand.ClearHand();
            seat.Hand.SetInteractable(false); // 🔒 no clicks while dealing
        }

        var order = seatRegistry.OrderAfter(dealer);
        var packets = (dealingFlowSettings && dealingFlowSettings.packetPattern != null && dealingFlowSettings.packetPattern.Length > 0)
                        ? dealingFlowSettings.packetPattern
                        : new int[] { 3, 2, 3 };

        foreach (var packet in packets)
        {
            foreach (var seat in order)
                yield return StartCoroutine(DealPacketToSeat(seatRegistry.Get(seat), packet));

            if (dealingFlowSettings && dealingFlowSettings.interPacketDelay > 0f)
                yield return new WaitForSeconds(dealingFlowSettings.interPacketDelay);
        }

        // 3) Final layout pass
        foreach (var s in seatRegistry.All())
            s.Hand.LayoutFan();

        // 4) Re-enable ONLY local seat after dealing
        foreach (var s in seatRegistry.All())
        {
            bool enable = s.IsLocal;
            s.Hand.SetInteractable(enable);

            if (enable)
            {
                // If LocalHandInput exists, make sure it hooks the new cards
                var input = s.Hand.GetComponent<LocalHandInput>();
                if (input) input.HookExistingCards();
            }
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

            // 1) create under Canvas (not under hand yet)
            var canvasRT = _canvas.transform as RectTransform;
            var cv = Instantiate(cardViewPrefab, canvasRT);

            // 2) visuals (force non-interactive during dealing)
            cv.SetCard(cardSO);
            cv.ShowFace(faceUpForSeat);
            cv.SetInteractable(false);          // 🔒 ALWAYS false during dealing

            // 3) temp add to hand & layout to get target slot in hand's anchored space
            hand.AddCard(cv);
            hand.LayoutFan();

            var handRT = (RectTransform)cv.transform;
            Vector2 targetAnchored = handRT.anchoredPosition;
            Quaternion targetRot   = handRT.localRotation;

            // 4) animate deck -> target slot via service
            Vector3 deckWorld = _deckAnchorRT.position;
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
    }
}
