using System;
using System.Collections.Generic;

public interface IBidSource
{
    void BeginBid(SeatId seat, Bid currentHigh, IReadOnlyList<Bid> allowed);
    event Action<Bid> OnBidChosen;
    void Cancel();
    bool IsHuman { get; }
}
