using UnityEngine;

public class DevDeckTester : MonoBehaviour
{
    public DeckManager deckManager;

    [ContextMenu("Test Shuffle + Deal 8")]
    void Test()
    {
        deckManager.ShuffleDeck();
        var cards = deckManager.DrawMultiple(8);
        foreach (var c in cards)
            Debug.Log($"Card: {c.DisplayName}");
    }
}
