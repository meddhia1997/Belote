using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Collects the 4 cards from TrickArea and stacks them into the winner's pile.
/// Works fully in Canvas space to avoid jumps when reparenting.
/// </summary>
public class TrickPileController : MonoBehaviour
{
    [Header("Scene Refs")]
    [Tooltip("RectTransform that contains the 4 played cards.")]
    public RectTransform trickArea;

    [Tooltip("Canvas root used for motion. If null, auto-finds parent Canvas.")]
    public RectTransform canvasRoot;

    [Header("Pile Anchors (under same Canvas hierarchy)")]
    public RectTransform southPileAnchor;
    public RectTransform westPileAnchor;
    public RectTransform northPileAnchor;
    public RectTransform eastPileAnchor;

    [Header("Animation")]
    public UIAnimationService animService;
    public CardAnimSettingsSO animSettings;

    [Header("Visual")]
    public float pileScale = 0.55f;
    public Vector2 pileStep = new Vector2(4f, -3f);
    public bool faceUpOnCollect = true;

    // runtime piles (per seat)
    private readonly Dictionary<SeatId, List<CardView>> piles = new()
    {
        { SeatId.South, new List<CardView>() },
        { SeatId.West,  new List<CardView>() },
        { SeatId.North, new List<CardView>() },
        { SeatId.East,  new List<CardView>()  },
    };

    public List<CardView> GetPile(SeatId seat) => piles[seat];

    public IEnumerator CollectTrick(SeatId winner)
    {
        if (!trickArea) { Debug.LogError("[TrickPile] trickArea not assigned."); yield break; }

        var canvasRT = ResolveCanvas();
        if (!canvasRT) { Debug.LogError("[TrickPile] Could not resolve Canvas RectTransform."); yield break; }

        var pileAnchor = GetAnchor(winner);
        if (!pileAnchor) { Debug.LogError($"[TrickPile] Pile anchor missing for seat {winner}."); yield break; }

        // Take a snapshot of CardViews currently under TrickArea.
        var trickCards = new List<CardView>(4);
        for (int i = 0; i < trickArea.childCount; i++)
        {
            var cv = trickArea.GetChild(i).GetComponent<CardView>();
            if (cv) trickCards.Add(cv);
            else Debug.LogWarning($"[TrickPile] Child '{trickArea.GetChild(i).name}' has no CardView.");
        }
        if (trickCards.Count == 0) yield break;

        // Target anchored position in CANVAS SPACE for the pile anchor
        Vector2 pileTargetCanvasPos = WorldToAnchored(canvasRT, pileAnchor.position);

        for (int i = 0; i < trickCards.Count; i++)
        {
            var cv = trickCards[i];
            var rt = (RectTransform)cv.transform;

            if (faceUpOnCollect) cv.ShowFace(true);

            // Reparent to canvas while keeping its screen position
            if (animService && canvasRT)
                animService.ReparentToCanvasKeepScreenPos(rt, canvasRT);
            else
                rt.SetParent(canvasRT, true); // keep world pos

            // Animate in canvas space to the pile anchor position + stack offset
            Vector2 stackOffset = pileStep * piles[winner].Count;
            Vector2 targetAnchoredCanvas = pileTargetCanvasPos + stackOffset;

            if (animService && animSettings)
                yield return animService.MoveTo(rt, targetAnchoredCanvas, animSettings);
            else
                rt.anchoredPosition = targetAnchoredCanvas;

            // Now that the card is visually at the pile spot (in canvas space),
            // parent it under the pile anchor and preserve the local offset so it
            // stays where it is relative to the pile.
            Vector2 localUnderPile = WorldToAnchored(pileAnchor, canvasRT.TransformPoint(targetAnchoredCanvas));
            rt.SetParent(pileAnchor, false);
            rt.anchoredPosition = localUnderPile;     // keep exact spot under pile
            rt.localRotation   = Quaternion.identity;
            rt.localScale      = Vector3.one * pileScale;

            piles[winner].Add(cv);
        }
    }

    // --- helpers ---
    RectTransform ResolveCanvas()
    {
        if (canvasRoot) return canvasRoot;
        var cv = trickArea ? trickArea.GetComponentInParent<Canvas>() : GetComponentInParent<Canvas>();
        return cv ? (RectTransform)cv.transform : null;
    }

    RectTransform GetAnchor(SeatId seat) => seat switch
    {
        SeatId.South => southPileAnchor,
        SeatId.West  => westPileAnchor,
        SeatId.North => northPileAnchor,
        SeatId.East  => eastPileAnchor,
        _ => null
    };

    /// <summary>
    /// Convert a world position into anchored coordinates of a target RectTransform.
    /// </summary>
    static Vector2 WorldToAnchored(RectTransform targetSpace, Vector3 worldPos)
    {
        Vector3 local = targetSpace.InverseTransformPoint(worldPos);
        return new Vector2(local.x, local.y);
    }
}
