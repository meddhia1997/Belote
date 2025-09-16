using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// IA très simple : probabilité de "Take" sinon "Pass".
/// Si "Take" : choisit le premier contrat autorisé dans 'allowed'.
/// </summary>
[CreateAssetMenu(fileName = "AI_BidderSimple", menuName = "Belote/Bidding/Sources/AI Simple")]
public class AI_BidderSimpleSO : ScriptableObject, IBidSource
{
    [Range(0f, 1f)] public float takeChance = 0.4f;
    public bool IsHuman => false;
    public event Action<Bid> OnBidChosen;

    public void BeginBid(SeatId seat, Bid currentHigh, IReadOnlyList<Bid> allowed)
    {
        if (UnityEngine.Random.value > takeChance || allowed == null || allowed.Count == 0)
        {
            OnBidChosen?.Invoke(Bid.Pass());
            return;
        }

        for (int i = 0; i < allowed.Count; i++)
        {
            if (allowed[i].type == BidType.Normal)
            {
                OnBidChosen?.Invoke(allowed[i]);
                return;
            }
        }
        OnBidChosen?.Invoke(Bid.Pass());
    }

    public void Cancel() { }
}
