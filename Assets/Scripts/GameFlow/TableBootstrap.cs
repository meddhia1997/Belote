using System.Collections.Generic;
using UnityEngine;

public class TableBootstrap : MonoBehaviour
{
    public DeckManager deckManager;
    public HandController southHand;
    public CardView cardViewPrefab;

    [ContextMenu("Start Offline Solo (Deal 8)")]
    public void StartOfflineSolo()
    {
        // reset + shuffle
        deckManager.ResetDeck();
        deckManager.ShuffleDeck();

        // clear UI then deal 8 to south
        southHand.ClearHand();
        List<CardDefinitionSO> cards = deckManager.DrawMultiple(8);

        foreach (var c in cards)
        {
            var cv = Instantiate(cardViewPrefab);
            cv.SetCard(c);                  // shows face
            southHand.AddCard(cv);
        }

        southHand.LayoutFan();
    }
}
