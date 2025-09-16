using UnityEngine;
using GameFlow.Bidding;

[CreateAssetMenu(fileName = "BidValidator_Classic", menuName = "Belote/Bidding/Validator/Classic")]
public class BidValidator_ClassicSO : ScriptableObject, IBidValidator
{
    public bool IsValid(Bid candidate, Bid current)
    {
        if (candidate.type == BidType.Pass) return true;
        if (current.type == BidType.Pass) return true;

        if (candidate.level > current.level) return true;
        if (candidate.level < current.level) return false;

        // même niveau : refuser même couleur (doit être supérieur en priorité)
        return candidate.suit != current.suit;
    }
}
