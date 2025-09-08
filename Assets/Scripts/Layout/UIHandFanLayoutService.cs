using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Fan/arc layout (like solitaire/card games).
/// </summary>
public class UIHandFanLayoutService : MonoBehaviour, IHandLayoutService
{
    public HandLayoutResult Layout(
        IReadOnlyList<CardView> cards,
        RectTransform handAnchor,
        HandLayoutSettingsSO settings
    )
    {
        var result = new HandLayoutResult { items = new List<HandLayoutItem>(cards.Count) };
        if (cards == null || cards.Count == 0 || !handAnchor) return result;

        float arc     = settings ? settings.arcDegrees  : 90f;
        float radius  = settings ? settings.radius      : 220f;
        float yBias   = settings ? settings.overlapLift : 0f;
        bool rotate   = settings ? settings.rotateCards : true;

        int n = cards.Count;
        float start = -arc * 0.5f;
        float step  = (n > 1) ? arc / (n - 1) : 0f;

        for (int i = 0; i < n; i++)
        {
            var cv = cards[i];
            if (!cv) continue;

            float angDeg = start + i * step;
            float theta = Mathf.Deg2Rad * (angDeg + 90f);

            Vector2 pos = new Vector2(
                radius * Mathf.Cos(theta),
                radius * Mathf.Sin(theta) + yBias
            );

            var rt = (RectTransform)cv.transform;
            rt.SetParent(handAnchor, false);
            rt.localScale = Vector3.one;
            rt.localRotation = rotate ? Quaternion.Euler(0, 0, angDeg) : Quaternion.identity;
            rt.anchoredPosition = pos;

            result.items.Add(new HandLayoutItem
            {
                card = cv,
                anchoredPos = pos,
                localRot = rt.localRotation
            });
        }

        return result;
    }
}
