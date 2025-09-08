using System.Collections;
using UnityEngine;

/// <summary>
/// Local player input for a single seat. Handles select and double tap to play.
/// Attach ONLY to the local seat's HandController GameObject.
/// </summary>
public class LocalHandInput : MonoBehaviour
{
    [Header("Refs")]
    public HandController hand;              // assign local seat hand
    public UIAnimationService animService;   // reuse same service
    public CardAnimSettingsSO animSettings;

    private CardView _selected;

    void Awake()
    {
        if (!hand) hand = GetComponent<HandController>();
    }

    void OnEnable()
    {
        if (hand != null)
        {
            hand.OnCardAdded += EnsureRelay;   // auto-hook new cards as they're dealt
            hand.SetInteractable(true);        // make sure local seat is clickable
            HookExistingCards();               // and hook any already-present cards
        }
    }

    void OnDisable()
    {
        if (hand != null)
            hand.OnCardAdded -= EnsureRelay;
    }

    /// <summary>Hook click relays for all existing cards (call after dealing too).</summary>
    public void HookExistingCards()
    {
        foreach (var cv in hand.GetCards())
            EnsureRelay(cv);
        hand.SetInteractable(true);
    }

    public void OnCardTapped(CardView card)
    {
        if (!card) return;

        if (_selected == card)
        {
            StartCoroutine(PlaySelected(card));
            return;
        }

        if (_selected) Deselect(_selected);
        Select(card);
    }

    private void Select(CardView card)
    {
        _selected = card;
        var rt = (RectTransform)card.transform;
        rt.SetAsLastSibling();
        if (animService && animSettings)
            StartCoroutine(animService.SelectUp(rt, animSettings));
    }

    private void Deselect(CardView card)
    {
        var rt = (RectTransform)card.transform;
        if (animService && animSettings)
            StartCoroutine(animService.SelectDown(rt, animSettings));
        _selected = null;
    }

    private IEnumerator PlaySelected(CardView card)
    {
        // Delegates to HandController (animation + removal + relayout)
        yield return StartCoroutine(hand.PlayCardToTrick(card));
        _selected = null;

        // TODO: TurnController.NotifyLocalPlayed(card);
    }

    private void EnsureRelay(CardView cv)
    {
        if (!cv) return;
        var relay = cv.GetComponent<CardClickRelay>();
        if (!relay) relay = cv.gameObject.AddComponent<CardClickRelay>();
        relay.input = this;   // route clicks to LocalHandInput
        relay.card  = cv;
    }
}
