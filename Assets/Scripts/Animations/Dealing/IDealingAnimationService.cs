using System.Collections;
using UnityEngine;

public interface IDealingAnimationService
{
    /// <summary>
    /// Animate one card from a deck world/anchored point into a hand slot (anchored pos + final rot).
    /// This method does ONLY movement/FX. Caller controls game state (add to hand, layout, etc.).
    /// </summary>
    IEnumerator AnimateCardFromDeckToHand(
        CardView cardView,
        RectTransform handAnchor,
        Vector3 deckWorldPos,
        Vector2 targetAnchored,
        Quaternion targetLocalRotation,
        Canvas canvas,
        UIAnimationService uiAnim,                 // low-level animator
        CardAnimSettingsSO cardAnimSettings,      // per-card timings/ease
        DealingFlowSettingsSO flowSettings        // rhythm/flow config
    );

    /// <summary>Utility: convert world position to anchored position in a target RectTransform.</summary>
    Vector2 WorldToAnchored(RectTransform targetParent, Vector3 world, Canvas canvas);
}
