using UnityEngine;

public interface ITrumpPolicy
{
    Suit CurrentTrump { get; }
    // later: void SetTrump(Suit s) from bidding
}
