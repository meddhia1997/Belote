using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the local player's hand (fan layout, tap-to-select, tap-to-play).
/// All animations are delegated to UIAnimationService (swappable later).
/// </summary>
public class HandController : MonoBehaviour
{
    [Header("Scene Refs")]
    [Tooltip("Bottom-center parent for the hand (this object or a child).")]
    public RectTransform handAnchor;               // HandAnchor_South
    [Tooltip("Center area where played cards should move to.")]
    public RectTransform trickArea;                // TrickArea

    [Header("Layout Settings")]
    public HandLayoutSettingsSO settings;          // arc, radius, etc.

    [Header("Animation (Service + Settings)")]
    public UIAnimationService animService;         // component in scene
    public CardAnimSettingsSO animSettings;        // timings + ease curve

    [Header("Prefabs")]
    public CardView cardViewPrefab;                // your CardView.prefab

    // Runtime
    private readonly List<CardView> _cards = new();
    private CardView _selected;

    // ------------- FAN LAYOUT -------------
    /// <summary>Repositions hand cards in an arc/fan.</summary>
    public void LayoutFan()
    {
        int n = _cards.Count;
        if (n == 0 || handAnchor == null) return;

        float arc = settings != null ? settings.arcDegrees : 90f;
        float start = -arc * 0.5f;
        float step = (n > 1) ? arc / (n - 1) : 0f;
        float radius = settings != null ? settings.radius : 220f;
        float yBias = settings != null ? settings.overlapLift : 0f;
        bool rotate = settings == null || settings.rotateCards;

        for (int i = 0; i < n; i++)
        {
            var cv = _cards[i];
            if (!cv) continue;

            var rt = (RectTransform)cv.transform;

            float angDeg = start + i * step;
            // 0° up: angle +90 to convert to UI canvas "up"
            float theta = Mathf.Deg2Rad * (angDeg + 90f);

            Vector2 pos = new Vector2(
                radius * Mathf.Cos(theta),
                radius * Mathf.Sin(theta) + yBias
            );

            // Parent (safe) and place
            rt.SetParent(handAnchor, false);
            rt.localScale = Vector3.one;
            rt.localRotation = rotate ? Quaternion.Euler(0, 0, angDeg) : Quaternion.identity;
            rt.anchoredPosition = pos;

            // reset interactable by default
            cv.SetInteractable(true);
        }
    }

    // ------------- INPUT: TAP -------------
    /// <summary>Called by CardClickRelay on the CardView.</summary>
    public void OnCardTapped(CardView card)
    {
        if (card == null) return;

        // Second tap on same card => confirm/play
        if (_selected == card)
        {
            StartCoroutine(PlayCardRoutine(card));
            return;
        }

        // Change selection
        if (_selected != null) Deselect(_selected);
        Select(card);
    }

    private void Select(CardView card)
    {
        _selected = card;
        var rt = (RectTransform)card.transform;
        rt.SetAsLastSibling(); // render on top
        StartCoroutine(animService.SelectUp(rt, animSettings));
    }

    private void Deselect(CardView card)
    {
        var rt = (RectTransform)card.transform;
        StartCoroutine(animService.SelectDown(rt, animSettings));
        _selected = null;
    }

    // ------------- PLAY FLOW -------------
    private IEnumerator PlayCardRoutine(CardView card)
    {
        // lock clicks on the played card immediately
        card.SetInteractable(false);

        var rt = (RectTransform)card.transform;

        // Reparent to top-level canvas while keeping screen position (so layout/rotation won't fight)
        var canvas = handAnchor.GetComponentInParent<Canvas>()?.transform as RectTransform;
        if (canvas != null)
            animService.ReparentToCanvasKeepScreenPos(rt, canvas);

        // Move to trick center (and optionally straighten rotation)
        yield return StartCoroutine(animService.MoveTo(rt, trickArea.anchoredPosition, animSettings));

        // reinforce non-interactability on table
        card.SetInteractable(false);

        // Remove from hand list
        _cards.Remove(card);

        // Keep the played card visible at the trick (reparent logically to TrickArea)
        rt.SetParent(trickArea, false);
        rt.localRotation = Quaternion.identity; // ensure straight at table

        // Clear selection + re-layout remaining cards
        _selected = null;
        LayoutFan();

        // TODO: notify turn controller / game flow to advance.
    }

    // ------------- BOOTSTRAP API -------------
    public void ClearHand()
    {
        for (int i = 0; i < _cards.Count; i++)
        {
            if (_cards[i]) Destroy(_cards[i].gameObject);
        }
        _cards.Clear();
        _selected = null;
    }

    public void AddCard(CardView cv)
    {
        if (!cv) return;

        // attach click relay (tap handling)
        var relay = cv.gameObject.GetComponent<CardClickRelay>();
        if (!relay) relay = cv.gameObject.AddComponent<CardClickRelay>();
        relay.controller = this;
        relay.card = cv;

        _cards.Add(cv);
    }

    // Optional utility if you need current cards
    public IReadOnlyList<CardView> GetCards() => _cards;
}
