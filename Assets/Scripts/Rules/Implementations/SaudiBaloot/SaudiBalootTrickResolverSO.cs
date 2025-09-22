using UnityEngine;

[CreateAssetMenu(fileName="SaudiBalootTrickResolver", menuName="Belote/Rules/Policies/TrickResolver/SaudiBaloot")]
public class SaudiBalootTrickResolverSO : ScriptableObject, ITrickResolver
{
    public (SeatId winner, int points) ResolveTrick(RulesContext ctx)
    {
        var trick   = ctx.CurrentTrick;
        var order   = ctx.Profile.OrderingPolicy;
        var scoring = ctx.Profile.ScoringPolicy;
        var trump   = ctx.Trump;
        var ToSuit  = ctx.ParseSuit;

        SeatId winner = trick.leader;
        var winCard = trick.cards[0].card;

        bool isSun = (trump == Suit.None);
        bool trumpSeen = (!isSun) && (ToSuit(winCard.Suit) == trump);

        // first card points
        int pts = scoring.GetCardPoints(ToSuit(winCard.Suit), winCard.Rank, trump);

        for (int i = 1; i < trick.cards.Count; i++)
        {
            var pc   = trick.cards[i];
            var suit = ToSuit(pc.card.Suit);
            bool isTrump = (!isSun) && (suit == trump);

            pts += scoring.GetCardPoints(suit, pc.card.Rank, trump);

            if (!isSun)
            {
                if (isTrump && !trumpSeen)
                {
                    trumpSeen = true; winner = pc.seat; winCard = pc.card; continue;
                }

                if (trumpSeen)
                {
                    if (isTrump && order.GetOrderValue(pc.card.Rank,true) > order.GetOrderValue(winCard.Rank,true))
                    { winner = pc.seat; winCard = pc.card; }
                }
                else
                {
                    if (suit == trick.leadSuit && order.GetOrderValue(pc.card.Rank,false) > order.GetOrderValue(winCard.Rank,false))
                    { winner = pc.seat; winCard = pc.card; }
                }
            }
            else
            {
                // Sun: only lead-suit order applies
                if (suit == trick.leadSuit && order.GetOrderValue(pc.card.Rank,false) > order.GetOrderValue(winCard.Rank,false))
                { winner = pc.seat; winCard = pc.card; }
            }
        }

        return (winner, pts);
    }
}
