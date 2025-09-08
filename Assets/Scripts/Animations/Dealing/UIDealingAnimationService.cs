using System.Collections;
using UnityEngine;

public class UIDealingAnimationService : MonoBehaviour, IDealingAnimationService
{
    public Vector2 WorldToAnchored(RectTransform targetParent, Vector3 world, Canvas canvas)
    {
        Camera cam = (canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : canvas.worldCamera;
        Vector2 screen = RectTransformUtility.WorldToScreenPoint(cam, world);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(targetParent, screen, cam, out var localPoint);
        return localPoint;
    }

    public IEnumerator AnimateCardFromDeckToHand(
        CardView cardView,
        RectTransform handAnchor,
        Vector3 deckWorldPos,
        Vector2 targetAnchored,
        Quaternion targetLocalRotation,
        Canvas canvas,
        UIAnimationService uiAnim,
        CardAnimSettingsSO cardAnimSettings,
        DealingFlowSettingsSO flowSettings
    )
    {
        if (!cardView || !handAnchor || !uiAnim || !cardAnimSettings) yield break;

        var rt = (RectTransform)cardView.transform;

        // Ensure we animate in hand's anchored space
        rt.SetParent(handAnchor, false);
        // Start from deck anchor (converted to this space)
        rt.anchoredPosition = WorldToAnchored(handAnchor, deckWorldPos, canvas);
        rt.localRotation = Quaternion.identity;

        // Move to slot
        yield return uiAnim.MoveTo(rt, targetAnchored, cardAnimSettings);
        rt.localRotation = targetLocalRotation;

        // Optional rhythm between cards
        if (flowSettings && flowSettings.interCardDelay > 0f)
            yield return new WaitForSeconds(flowSettings.interCardDelay);
    }
}
