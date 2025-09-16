public interface ITrumpSource
{
    bool IsImmediate { get; }                 // true if DecideTrump can return instantly (no UI/bidding)
    Suit DecideTrump(SeatId dealer);          // return chosen suit
}
