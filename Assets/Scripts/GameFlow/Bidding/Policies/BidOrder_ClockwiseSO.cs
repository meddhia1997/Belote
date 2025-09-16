using System.Collections.Generic;
using UnityEngine;
using GameFlow.Bidding;

[CreateAssetMenu(fileName = "BidOrder_Clockwise", menuName = "Belote/Bidding/Order/Clockwise")]
public class BidOrder_ClockwiseSO : ScriptableObject, IBidOrderPolicy
{
    public IEnumerable<SeatId> EnumerateOrder(SeatId dealer)
    {
        var start = SeatRegistry.Next(dealer);
        return new List<SeatId>
        {
            start,
            SeatRegistry.Next(start),
            SeatRegistry.Next(SeatRegistry.Next(start)),
            SeatRegistry.Next(SeatRegistry.Next(SeatRegistry.Next(start)))
        };
    }
}
