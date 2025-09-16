using UnityEngine;
using GameFlow.Bidding;

[CreateAssetMenu(fileName = "BidComparator_Classic", menuName = "Belote/Bidding/Comparator/Classic")]
public class BidComparator_ClassicSO : ScriptableObject, IBidComparator
{
    [SerializeField] private Suit[] suitPriority = new[] { Suit.Hearts, Suit.Diamonds, Suit.Clubs, Suit.Spades };

    public bool IsBetter(Bid candidate, Bid current)
    {
        if (candidate.type == BidType.Pass) return false;
        if (current.type == BidType.Pass) return true;

        if (candidate.level != current.level)
            return candidate.level > current.level;

        int ci = IndexOf(suitPriority, candidate.suit);
        int hi = IndexOf(suitPriority, current.suit);
        return ci >= 0 && hi >= 0 && ci < hi; // plus petite position = plus prioritaire
    }

    int IndexOf(Suit[] arr, Suit s)
    {
        for (int i = 0; i < arr.Length; i++) if (arr[i] == s) return i;
        return int.MaxValue;
    }
}
