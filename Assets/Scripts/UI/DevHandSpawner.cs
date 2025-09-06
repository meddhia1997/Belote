using System.Collections.Generic;
using UnityEngine;

public class DevHandSpawner : MonoBehaviour
{
    [Header("Refs")]
    public DeckManager deckManager;         // assign from scene
    public CardView cardViewPrefab;         // assign prefab (UI version)
    public RectTransform handParent;        // a UI panel under Canvas

    [Header("Settings")]
    public int handSize = 8;
    public bool clearBeforeDeal = true;
    public bool shuffleBeforeDeal = true;

    [ContextMenu("Deal New Hand")]
    public void DealNewHand()
    {
        if (deckManager == null || cardViewPrefab == null || handParent == null)
        {
            Debug.LogError("[DevHandSpawner] Missing references.");
            return;
        }

        if (shuffleBeforeDeal) deckManager.ShuffleDeck();

        var cards = deckManager.DrawMultiple(handSize);
        if (cards == null || cards.Count == 0)
        {
            Debug.LogWarning("[DevHandSpawner] Deck empty or not set.");
            return;
        }

        if (clearBeforeDeal)
        {
            for (int i = handParent.childCount - 1; i >= 0; i--)
                DestroyImmediate(handParent.GetChild(i).gameObject);
        }

        for (int i = 0; i < cards.Count; i++)
        {
            var go = Instantiate(cardViewPrefab, handParent);
            go.name = $"CardView_{i+1}";
            go.transform.localScale = Vector3.one;

            // bind data -> shows the face sprite
            go.SetCard(cards[i]);
        }

        Debug.Log($"[DevHandSpawner] Spawned {cards.Count} cards.");
    }
}
