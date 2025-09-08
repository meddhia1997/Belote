using UnityEngine;

[CreateAssetMenu(fileName="ClassicTrickResolver", menuName="Belote/Rules/Policies/TrickResolver/Classic")]
public class ClassicTrickResolverSO : ScriptableObject, ITrickResolver
{
    public (SeatId winner, int points) ResolveTrick(RulesContext ctx)
    {
        var trick = ctx.CurrentTrick;
        var order = ctx.Profile.OrderingPolicy;
        var scoring = ctx.Profile.ScoringPolicy;
        var trump = ctx.Trump;
        var ToSuit = ctx.ParseSuit;

        SeatId winner = trick.leader;
        var winCard = trick.cards[0].card;
        bool trumpSeen = (ToSuit(winCard.Suit) == trump);

        int pts = scoring.GetCardPoints(ToSuit(winCard.Suit), winCard.Rank, trump);

        for (int i = 1; i < trick.cards.Count; i++)
        {
            var pc = trick.cards[i];
            var suit = ToSuit(pc.card.Suit);
            bool isTrump = (suit == trump);

            pts += scoring.GetCardPoints(suit, pc.card.Rank, trump);

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

        // last trick bonus is applied by round controller once round ends (or you can detect if ctx.TrickIndex==7 and add here)
        return (winner, pts);
    }
}
