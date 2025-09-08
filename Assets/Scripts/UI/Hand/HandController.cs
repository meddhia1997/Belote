using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Seat hand UI: owns card list, layout, and play-to-trick animation.
/// NO input here. Input is handled by LocalHandInput on the local seat only.
/// Raises OnCardAdded so LocalHandInput can auto-hook relays for new cards.
/// </summary>
public class HandController : MonoBehaviour, IHandView
{
    [Header("Scene Refs")]
    public RectTransform handAnchor;    // anchor for this hand
    public RectTransform trickArea;     // center trick area

    [Header("Layout")]
    public HandLayoutSettingsSO settings;
    public UIHandFanLayoutService layoutService;   // any IHandLayoutService impl

    [Header("Animation")]
    public UIAnimationService animService;
    public CardAnimSettingsSO animSettings;

    [Header("Prefabs")]
    public CardView cardViewPrefab;

    // Runtime
    private readonly List<CardView> _cards = new();

    // Events
    public event Action<CardView> OnCardAdded;   // <-- notifies when a card is added to this hand

    // IHandView
    public RectTransform HandAnchor => handAnchor;

    public void SetInteractable(bool interactable)
    {
        for (int i = 0; i < _cards.Count; i++)
            if (_cards[i]) _cards[i].SetInteractable(interactable);
    }

    // -- Layout (delegated) --
    public void LayoutFan()
    {
        if (!layoutService)
        {
            Debug.LogWarning("[HandController] No layoutService assigned.");
            return;
        }
        layoutService.Layout(_cards, handAnchor, settings);
    }

    // -- Play animation (called by LocalHandInput / AI / Net) --
    public IEnumerator PlayCardToTrick(CardView card)
    {
        if (!card) yield break;

        card.SetInteractable(false);
        var rt = (RectTransform)card.transform;

        var canvas = handAnchor.GetComponentInParent<Canvas>()?.transform as RectTransform;
        if (canvas && animService)
            animService.ReparentToCanvasKeepScreenPos(rt, canvas);

        if (animService && animSettings)
            yield return animService.MoveTo(rt, trickArea.anchoredPosition, animSettings);

        _cards.Remove(card);
        rt.SetParent(trickArea, false);
        rt.localRotation = Quaternion.identity;

        LayoutFan();
    }

    // -- API for dealing/flow --
    public void ClearHand()
    {
        foreach (var c in _cards) if (c) Destroy(c.gameObject);
        _cards.Clear();
    }

    public void AddCard(CardView cv)
    {
        if (!cv) return;
        // Do NOT attach click relays here; LocalHandInput will do it for local seat only
        _cards.Add(cv);

        // Notify listeners (LocalHandInput) so it can attach relays immediately
        OnCardAdded?.Invoke(cv);
    }

    public IReadOnlyList<CardView> GetCards() => _cards;
}
