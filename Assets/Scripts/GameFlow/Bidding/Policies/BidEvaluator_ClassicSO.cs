using System.Collections.Generic;
using UnityEngine;
using GameFlow.Bidding;

[CreateAssetMenu(fileName = "BidEvaluator_Classic", menuName = "Belote/Bidding/Evaluator/Classic")]
public class BidEvaluator_ClassicSO : ScriptableObject, IBidEvaluator
{
    public List<Bid> BuildAllowed(Bid currentHigh)
    {
        // Basique : Pass + 4 couleurs niveau 1
        return new List<Bid>
        {
            Bid.Pass(),
            Bid.Normal(Suit.Hearts, 1),
            Bid.Normal(Suit.Diamonds, 1),
            Bid.Normal(Suit.Clubs, 1),
            Bid.Normal(Suit.Spades, 1),
        };
    }
}
