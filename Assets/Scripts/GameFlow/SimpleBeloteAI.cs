using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple trump-aware Belote AI:
/// - Considers trump (ctx.Trump), lead suit, ordering & scoring policies
/// - Chooses minimum winning card when needed, otherwise discards low points
/// </summary>
[CreateAssetMenu(fileName = "SimpleBeloteAI", menuName = "Belote/AI/Simple Trump Aware")]
public class SimpleBeloteAI_SO : ScriptableObject, IAgentAI
{
    [Header("Tuning")]
    [Tooltip("Prefer to keep 10/A when discarding if possible.")]
    public bool protectHighPointCards = true;

    public CardDefinitionSO ChooseCard(RulesContext ctx, List<CardDefinitionSO> legal, SeatId seat)
    {
        if (legal == null || legal.Count == 0) return null;

        var order   = ctx.Profile.OrderingPolicy;
        var scoring = ctx.Profile.ScoringPolicy;
        var trump   = ctx.Trump;

        // 1) If leading the trick: pick a safe lead
        if (ctx.CurrentTrick.cards.Count == 0)
            return LeadChoice(legal, order, scoring, trump);

        // 2) Otherwise, we’re following
        var currentWinnerSeat = CurrentWinnerSeat(ctx);
        bool partnerWinning   = ArePartners(seat, currentWinnerSeat);

        if (partnerWinning)
        {
            // Partner is already winning → discard lowest points (keep 10/A if possible)
            return DiscardLowestPoints(legal, scoring, trump, protectHighPointCards);
        }

        // Try to win with the minimum investment
        var winnerCard = MinimalWinningCard(legal, ctx, order);
        if (winnerCard != null) return winnerCard;

        // Cannot win → discard lowest points
        return DiscardLowestPoints(legal, scoring, trump, protectHighPointCards);
    }

    // ---------- Helpers ----------

    CardDefinitionSO LeadChoice(List<CardDefinitionSO> legal, IOrderingPolicy order, IScoringPolicy scoring, Suit trump)
    {
        // Prefer leading a non-trump, longest suit, lowest order value (save points & trumps)
        var bySuit = GroupBySuit(legal);
        CardDefinitionSO best = null;
        int bestLen = -1;
        foreach (var kv in bySuit)
        {
            if (kv.Key == trump) continue; // avoid leading trumps at simple level
            var suitLen = kv.Value.Count;
            if (suitLen > bestLen)
            {
                bestLen = suitLen;
                best = LowestByOrder(kv.Value, order, isTrump: false);
            }
        }
        // If all are trump or only one suit, just play the lowest by order among legal
        if (best == null)
        {
            bool anyTrump = false;
            foreach (var c in legal) if (SuitUtils.Parse(c.Suit) == trump) { anyTrump = true; break; }
            best = LowestByOrder(legal, order, anyTrump);
        }
        return best;
    }

    CardDefinitionSO MinimalWinningCard(List<CardDefinitionSO> legal, RulesContext ctx, IOrderingPolicy order)
    {
        var leadSuit   = ctx.CurrentTrick.leadSuit;
        var trump      = ctx.Trump;

        // Compute the current winning card on table using same compare logic as resolver
        var (curWinnerSeat, curWinnerCard) = CurrentWinner(ctx);
        if (curWinnerCard == null) return null;

        bool trumpSeen = SuitUtils.Parse(curWinnerCard.Suit) == trump;

        CardDefinitionSO best = null;
        int bestOrderVal = int.MaxValue;

        foreach (var c in legal)
        {
            var suit = SuitUtils.Parse(c.Suit);
            bool isTrump = (suit == trump);

            // Can this card beat current winner?
            if (trumpSeen)
            {
                if (!isTrump) continue; // only trump beats trump
                if (order.GetOrderValue(c.Rank, true) > order.GetOrderValue(curWinnerCard.Rank, true))
                {
                    int v = order.GetOrderValue(c.Rank, true);
                    if (v < bestOrderVal) { bestOrderVal = v; best = c; }
                }
            }
            else
            {
                if (isTrump)
                {
                    // Any trump beats non-trump (be minimal trump)
                    int v = order.GetOrderValue(c.Rank, true);
                    if (v < bestOrderVal) { bestOrderVal = v; best = c; }
                }
                else if (suit == leadSuit)
                {
                    if (order.GetOrderValue(c.Rank, false) > order.GetOrderValue(curWinnerCard.Rank, false))
                    {
                        int v = order.GetOrderValue(c.Rank, false);
                        if (v < bestOrderVal) { bestOrderVal = v; best = c; }
                    }
                }
            }
        }
        return best;
    }

    (SeatId seat, CardDefinitionSO card) CurrentWinner(RulesContext ctx)
    {
        var trick = ctx.CurrentTrick;
        var order = ctx.Profile.OrderingPolicy;
        var trump = ctx.Trump;

        SeatId winner = trick.leader;
        var winCard   = trick.cards[0].card;
        bool trumpSeen = (SuitUtils.Parse(winCard.Suit) == trump);

        for (int i = 1; i < trick.cards.Count; i++)
        {
            var pc = trick.cards[i];
            var suit = SuitUtils.Parse(pc.card.Suit);
            bool isTrump = (suit == trump);

            if (isTrump && !trumpSeen)
            {
                trumpSeen = true; winner = pc.seat; winCard = pc.card; continue;
            }

            if (trumpSeen)
            {
                if (isTrump && order.GetOrderValue(pc.card.Rank, true) > order.GetOrderValue(winCard.Rank, true))
                { winner = pc.seat; winCard = pc.card; }
            }
            else
            {
                if (suit == trick.leadSuit && order.GetOrderValue(pc.card.Rank, false) > order.GetOrderValue(winCard.Rank, false))
                { winner = pc.seat; winCard = pc.card; }
            }
        }
        return (winner, winCard);
    }

    SeatId CurrentWinnerSeat(RulesContext ctx) => CurrentWinner(ctx).seat;

    CardDefinitionSO DiscardLowestPoints(List<CardDefinitionSO> legal, IScoringPolicy scoring, Suit trump, bool protectHigh)
    {
        // Try to discard a low-point non-trump; if forced, discard lowest-point among all.
        CardDefinitionSO best = null;
        int bestPts = int.MaxValue;

        foreach (var c in legal)
        {
            var suit = SuitUtils.Parse(c.Suit);
            int pts = scoring.GetCardPoints(suit, c.Rank, trump);

            // Protect 10/A if possible
            if (protectHigh && (c.Rank == "10" || c.Rank == "A")) pts += 50;

            // Prefer non-trump for discard
            if (suit != trump) pts -= 1;

            if (pts < bestPts) { bestPts = pts; best = c; }
        }
        return best ?? legal[0];
    }

    Dictionary<Suit, List<CardDefinitionSO>> GroupBySuit(List<CardDefinitionSO> cards)
    {
        var map = new Dictionary<Suit, List<CardDefinitionSO>>();
        foreach (var c in cards)
        {
            var s = SuitUtils.Parse(c.Suit);
            if (!map.TryGetValue(s, out var list)) { list = new List<CardDefinitionSO>(); map[s] = list; }
            list.Add(c);
        }
        return map;
    }

    CardDefinitionSO LowestByOrder(List<CardDefinitionSO> cards, IOrderingPolicy order, bool isTrump)
    {
        CardDefinitionSO low = null; int best = int.MaxValue;
        foreach (var c in cards)
        {
            int v = order.GetOrderValue(c.Rank, isTrump);
            if (v < best) { best = v; low = c; }
        }
        return low;
    }

    bool ArePartners(SeatId a, SeatId b)
    {
        return (a == SeatId.South && b == SeatId.North) ||
               (a == SeatId.North && b == SeatId.South) ||
               (a == SeatId.West  && b == SeatId.East ) ||
               (a == SeatId.East  && b == SeatId.West );
    }
}
