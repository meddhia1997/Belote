using UnityEngine;
using System.Collections.Generic;

public class DeckManager : MonoBehaviour
{
    [Header("Deck Reference")]
    public DeckDefinitionSO deckDefinition;

    private List<CardDefinitionSO> runtimeDeck;

    void Awake()
    {
        ResetDeck();
    }

    [ContextMenu("Reset Deck")]
    public void ResetDeck()
    {
        runtimeDeck = new List<CardDefinitionSO>(deckDefinition.Cards);
    }

    [ContextMenu("Shuffle Deck")]
    public void ShuffleDeck()
    {
        System.Random rng = new System.Random();
        int n = runtimeDeck.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (runtimeDeck[k], runtimeDeck[n]) = (runtimeDeck[n], runtimeDeck[k]);
        }
        Debug.Log("Deck shuffled.");
    }

    [ContextMenu("Draw 8 Cards")]
    public void Draw8Test()
    {
        var drawn = DrawMultiple(8);
        foreach (var c in drawn)
            Debug.Log($"Drew: {c.DisplayName}");
    }

    public CardDefinitionSO DrawCard()
    {
        if (runtimeDeck.Count == 0) return null;
        var card = runtimeDeck[0];
        runtimeDeck.RemoveAt(0);
        return card;
    }

    public List<CardDefinitionSO> DrawMultiple(int count)
    {
        var cards = new List<CardDefinitionSO>();
        for (int i = 0; i < count && runtimeDeck.Count > 0; i++)
            cards.Add(DrawCard());
        return cards;
    }
}
