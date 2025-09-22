using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName="SaudiBalootLegalMovePolicy", menuName="Belote/Rules/Policies/LegalMoves/SaudiBaloot")]
public class SaudiBalootLegalMovePolicySO : ScriptableObject, ILegalMovePolicy
{
    [Header("Constraints")]
    public bool mustFollowSuit = true;
    public bool mustTrumpIfVoid = true;     // ignored when trump == Suit.None (Sun)
    public bool mustOvertrump = true;
    public bool canDiscardIfPartnerWinning = false;

    public List<CardDefinitionSO> GetLegalMoves(
        RulesContext ctx,
        IReadOnlyList<CardDefinitionSO> hand,
        SeatId seatToPlay)
    {
        var trick = ctx.CurrentTrick;
        var trump = ctx.Trump;
        var ToSuit = ctx.ParseSuit;

        if (trick.cards.Count == 0)
            return new List<CardDefinitionSO>(hand);

        var lead = trick.leadSuit;
        var follow = FilterBySuit(hand, lead, ToSuit);

        // 1) Must follow lead
        if (mustFollowSuit && follow.Count > 0)
            return follow;

        // 2) Void in lead → trump logic (disabled in Sun)
        bool isSun = (trump == Suit.None);
        if (!isSun && mustTrumpIfVoid)
        {
            var trumps = FilterBySuit(hand, trump, ToSuit);
            if (trumps.Count > 0)
            {
                bool trumpInTrick = HasSuit(trick.cards, trump, ToSuit);
                if (mustOvertrump && trumpInTrick)
                {
                    var highestTrump = HighestTrumpInTrick(trick.cards, trump, ctx.Profile.OrderingPolicy, ToSuit);
                    var over = HigherTrumps(trumps, highestTrump, ctx.Profile.OrderingPolicy);
                    if (over.Count > 0) return over;
                    if (canDiscardIfPartnerWinning && IsPartnerWinning(trick, seatToPlay, ctx))
                        return trumps; // free trump
                    return trumps;     // must still play a trump (variant choice)
                }
                return trumps; // first trump in trick
            }
        }

        // 3) Discard (Sun or no trump available)
        return new List<CardDefinitionSO>(hand);
    }

    // helpers
    List<CardDefinitionSO> FilterBySuit(IReadOnlyList<CardDefinitionSO> src, Suit s, System.Func<string,Suit> ToSuit)
    {
        var list = new List<CardDefinitionSO>();
        foreach (var c in src) if (ToSuit(c.Suit) == s) list.Add(c);
        return list;
    }
    bool HasSuit(List<PlayedCard> cards, Suit s, System.Func<string,Suit> ToSuit)
    {
        for (int i=0;i<cards.Count;i++) if (ToSuit(cards[i].card.Suit)==s) return true;
        return false;
    }
    CardDefinitionSO HighestTrumpInTrick(List<PlayedCard> cards, Suit trump, IOrderingPolicy order, System.Func<string,Suit> ToSuit)
    {
        CardDefinitionSO best=null; int bestVal=-1;
        foreach (var pc in cards)
        {
            if (ToSuit(pc.card.Suit)!=trump) continue;
            int v = order.GetOrderValue(pc.card.Rank, true);
            if (v>bestVal){bestVal=v; best=pc.card;}
        }
        return best;
    }
    List<CardDefinitionSO> HigherTrumps(List<CardDefinitionSO> trumps, CardDefinitionSO than, IOrderingPolicy order)
    {
        if (than==null) return new List<CardDefinitionSO>(trumps);
        var list=new List<CardDefinitionSO>();
        int th = order.GetOrderValue(than.Rank, true);
        foreach (var c in trumps) if (order.GetOrderValue(c.Rank,true) > th) list.Add(c);
        return list;
    }
    bool IsPartnerWinning(Trick trick, SeatId me, RulesContext ctx)
    {
        var (winner, _) = ctx.Profile.TrickResolver.ResolveTrick(ctx);
        return ArePartners(me, winner);
    }
    bool ArePartners(SeatId a, SeatId b)
    {
        return (a==SeatId.South && b==SeatId.North) || (a==SeatId.North && b==SeatId.South)
            || (a==SeatId.West && b==SeatId.East) || (a==SeatId.East && b==SeatId.West);
    }
}
