using System.Collections.Generic;
using UnityEngine;

public interface IHandLayoutService
{
    /// <summary>
    /// Computes and applies a layout for the given cards under the provided handAnchor.
    /// Returns the computed results for further use (e.g., previews, animations).
    /// </summary>
    HandLayoutResult Layout(
        IReadOnlyList<CardView> cards,
        RectTransform handAnchor,
        HandLayoutSettingsSO settings
    );
}

public struct HandLayoutItem
{
    public CardView card;
    public Vector2 anchoredPos;
    public Quaternion localRot;
}

public struct HandLayoutResult
{
    public List<HandLayoutItem> items;
}
