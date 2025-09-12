using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Local player's input: selection and confirm. Does NOT move cards itself.
/// It emits OnConfirmPlay(cardDef). TurnFlowController performs the animation.
/// </summary>
public class LocalHandInput : MonoBehaviour
{
    public HandController hand;
    public UIAnimationService animService;
    public CardAnimSettingsSO animSettings;

    public event Action<CardDefinitionSO> OnConfirmPlay;

    private CardView _selected;
    private HashSet<CardDefinitionSO> _legal; // null = unrestricted

    void Awake()
    {
        if (!hand) hand = GetComponent<HandController>();
        HookExistingCards();
    }

    public void SetLegal(HashSet<CardDefinitionSO> legal)
    {
        _legal = legal;
        // Optional: add highlight on allowed cards
        UpdateHighlights();
    }

    private void UpdateHighlights()
    {
        var cards = hand.GetCards();
        for (int i = 0; i < cards.Count; i++)
        {
            var cv = cards[i];
            if (!cv) continue;
            bool allowed = (_legal == null) || _legal.Contains(cv.GetCardDefinition());
            cv.SetInteractable(allowed && hand == hand); // enable raycast only if allowed
            // optional: cv.SetHighlight(allowed);
        }
    }

    public void HookExistingCards()
    {
        foreach (var cv in hand.GetCards())
            EnsureRelay(cv);
        hand.SetInteractable(true);
    }

    public void OnCardTapped(CardView card)
    {
        if (!card) return;

        // block illegal
        if (_legal != null && !_legal.Contains(card.GetCardDefinition()))
            return;

        if (_selected == card)
        {
            // confirm
            OnConfirmPlay?.Invoke(card.GetCardDefinition());
            // don't animate here; TurnFlow handles it
            _selected = null;
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

    private void EnsureRelay(CardView cv)
    {
        var relay = cv.GetComponent<CardClickRelay>();
        if (!relay) relay = cv.gameObject.AddComponent<CardClickRelay>();
        relay.input = this;
        relay.card  = cv;
    }
}
